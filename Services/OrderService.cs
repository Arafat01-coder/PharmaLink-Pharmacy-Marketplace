using System.Data;
using Microsoft.Data.SqlClient;
using PharmaLinkApp.Database;
using PharmaLinkApp.Models;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Checkout, order history, invoices and order status.
    ///
    /// Checkout is the most important piece of the system. Six things have to
    /// happen together - the order header, one line per cart row, the
    /// prescription row when one is needed, the stock reduction, the cart clean
    /// up and the frozen commission - and if any one of them failed on its own
    /// the database would be left with an order that has no items, an Rx order
    /// with no prescription, or stock that was sold twice. Wrapping them in a
    /// single transaction is what makes the checkout safe.
    /// </summary>
    public class OrderService
    {
        private readonly DbHelper _db = new DbHelper();

        /// <summary>
        /// The prescription state of an order, as one of exactly five words:
        /// 'Not needed' (no RequiresRx medicine on it), 'Missing' (needs one, none
        /// uploaded), or the VerifyStatus of the CURRENT prescription - the row
        /// with the highest PrescriptionId - 'Pending', 'Approved' or 'Rejected'.
        /// Expects the order aliased as o and the current prescription supplied
        /// by <see cref="CurrentRxApply"/> as rx.
        /// </summary>
        private const string RxStateExpression = @"
        CASE WHEN NOT EXISTS (SELECT 1 FROM OrderItems xi
                              INNER JOIN Medicines xm ON xm.MedicineId = xi.MedicineId
                              WHERE xi.OrderId = o.OrderId AND xm.RequiresRx = 1)
                  THEN 'Not needed'
             WHEN rx.VerifyStatus IS NULL THEN 'Missing'
             ELSE rx.VerifyStatus END";

        /// <summary>The order's current (newest) prescription, as rx.</summary>
        private const string CurrentRxApply = @"
        OUTER APPLY (SELECT TOP 1 p.VerifyStatus, p.RejectReason
                     FROM   Prescriptions p
                     WHERE  p.OrderId = o.OrderId
                     ORDER BY p.PrescriptionId DESC) rx";

        // =====================================================================
        //  CHECKOUT  (requirements 25 and 26)
        // =====================================================================

        /// <summary>
        /// Places one order for one pharmacy's slice of the basket. A cart that
        /// spans two pharmacies is checked out by calling this once per
        /// pharmacy, which is why every statement below carries @PharmacyId.
        ///
        /// <paramref name="paymentMobile"/> is the bKash or Nagad wallet number
        /// and is stored only for those two methods. When the slice contains a
        /// RequiresRx medicine, <paramref name="prescriptionImagePath"/> must name
        /// the customer's image: it is copied into the upload folder BEFORE the
        /// transaction (so no file I/O happens while rows are locked), its row is
        /// written INSIDE the transaction, and the copy is deleted again if the
        /// transaction does not commit. If the copy itself fails, no order is
        /// placed.
        ///
        /// Returns the new OrderId, or 0 with a readable message when the order
        /// could not be placed.
        /// </summary>
        public int Checkout(int customerId, int pharmacyId, string deliveryAddress,
                            string paymentMethod, string paymentMobile, decimal deliveryCharge,
                            string prescriptionImagePath, string doctorName, out string message)
        {
            bool walletPayment = paymentMethod == "bKash" || paymentMethod == "Nagad";
            string mobile = walletPayment && !string.IsNullOrWhiteSpace(paymentMobile) ? paymentMobile.Trim() : null;

            if (walletPayment && mobile == null)
            {
                message = "Enter the " + paymentMethod + " number the payment will come from.";
                return 0;
            }

            // ---- 0. copy the prescription image before anything is locked ----
            string storedPath = null, storedFullPath = null;
            if (!string.IsNullOrWhiteSpace(prescriptionImagePath))
            {
                string copyMessage;
                if (!PrescriptionService.TryStoreImage(prescriptionImagePath, "rx-c" + customerId,
                                                       out storedPath, out storedFullPath, out copyMessage))
                {
                    message = copyMessage + " Your order has not been placed.";
                    return 0;
                }
            }

            bool committed = false;
            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();

                    // READ COMMITTED is enough: the stock rows are read WITH
                    // (UPDLOCK, ROWLOCK) below, and an update lock is held until
                    // the transaction ends, so a second checkout for the same
                    // medicine waits here and then sees the reduced stock. The
                    // old SERIALIZABLE level also took range locks on Cart and
                    // Offers that only caused needless deadlocks.
                    using (SqlTransaction tx = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            // ---- 1. lock and re-check this pharmacy's cart lines ----
                            // Two customers can reach the checkout for the last unit
                            // of a medicine at the same time, and a pharmacy can be
                            // suspended or a medicine delisted while the customer is
                            // on this screen. Checking again here, under lock,
                            // rather than trusting what the cart screen showed, is
                            // what stops the same unit being sold twice or an
                            // unbuyable medicine being sold at all.
                            const string lockAndCheck = @"
SELECT  m.MedicineId, m.MedicineName, ct.Quantity, m.Stock, m.IsActive, m.RequiresRx,
        CASE WHEN m.ExpiryDate > CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END AS IsExpired,
        ph.Status AS PharmacyStatus
FROM    Cart ct WITH (UPDLOCK, ROWLOCK)
        INNER JOIN Medicines  m  WITH (UPDLOCK, ROWLOCK) ON m.MedicineId  = ct.MedicineId
        INNER JOIN Pharmacies ph                         ON ph.PharmacyId = m.PharmacyId
WHERE   ct.CustomerId = @CustomerId
  AND   m.PharmacyId  = @PharmacyId;";

                            DataTable lines = new DataTable();
                            using (SqlCommand cmd = new SqlCommand(lockAndCheck, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                                cmd.Parameters.AddWithValue("@PharmacyId", pharmacyId);
                                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                                {
                                    adapter.Fill(lines);
                                }
                            }

                            string problem = FindCheckoutProblem(lines, storedPath != null);
                            if (problem != null)
                            {
                                tx.Rollback();
                                message = problem;
                                return 0;
                            }

                            bool needsRx = lines.Rows.Cast<DataRow>().Any(r => DbHelper.GetBool(r, "RequiresRx"));

                            // ---- 2. header, lines, prescription, stock, cart ----
                            // The unit price is rounded to paisa once, into @Lines,
                            // and every total is summed from that table. That is
                            // what makes Orders.ItemsTotal equal SUM(OrderItems.Subtotal)
                            // to the paisa, and the commission is taken from that
                            // same total.
                            const string placeOrder = @"
DECLARE @Lines TABLE (MedicineId INT PRIMARY KEY, Quantity INT NOT NULL, UnitPrice DECIMAL(10,2) NOT NULL);

INSERT INTO @Lines (MedicineId, Quantity, UnitPrice)
SELECT  ct.MedicineId, ct.Quantity,
        CAST(ROUND(m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0), 2) AS DECIMAL(10,2))
FROM    Cart ct
        INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId
        OUTER APPLY (SELECT MAX(o.DiscountPercent) AS Pct
                     FROM   Offers o
                     WHERE  o.MedicineId = m.MedicineId AND o.IsActive = 1
                       AND  CAST(GETDATE() AS DATE) BETWEEN o.StartDate AND o.EndDate) d
WHERE   ct.CustomerId = @CustomerId AND m.PharmacyId = @PharmacyId;

DECLARE @Total DECIMAL(12,2) = (SELECT CAST(SUM(Quantity * UnitPrice) AS DECIMAL(12,2)) FROM @Lines);
DECLARE @CommRate DECIMAL(5,2) = (SELECT CommissionRate FROM Pharmacies WHERE PharmacyId = @PharmacyId);

INSERT INTO Orders (CustomerId, PharmacyId, ItemsTotal, DeliveryCharge, CommissionAmount,
                    DeliveryAddress, PaymentMethod, PaymentMobile, Status)
VALUES (@CustomerId, @PharmacyId, @Total, @DeliveryCharge,
        CAST(ROUND(@Total * @CommRate / 100.0, 2) AS DECIMAL(12,2)),
        @DeliveryAddress, @PaymentMethod, @PaymentMobile, 'Placed');

DECLARE @NewOrderId INT = CAST(SCOPE_IDENTITY() AS INT);

-- one line per cart row, at the discounted price the customer actually saw
INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice)
SELECT  @NewOrderId, MedicineId, Quantity, UnitPrice FROM @Lines;

-- the prescription is part of the order, so it commits or rolls back with it
IF @ImagePath IS NOT NULL
    INSERT INTO Prescriptions (OrderId, CustomerId, ImagePath, DoctorName)
    VALUES (@NewOrderId, @CustomerId, @ImagePath, @DoctorName);

-- take the stock off the shelf
UPDATE  m SET m.Stock = m.Stock - l.Quantity
FROM    Medicines m INNER JOIN @Lines l ON l.MedicineId = m.MedicineId;

-- clear only this pharmacy's lines; the rest of the basket becomes the next order
DELETE  ct
FROM    Cart ct INNER JOIN @Lines l ON l.MedicineId = ct.MedicineId
WHERE   ct.CustomerId = @CustomerId;

SELECT @NewOrderId;";

                            int newOrderId;
                            using (SqlCommand cmd = new SqlCommand(placeOrder, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                                cmd.Parameters.AddWithValue("@PharmacyId", pharmacyId);
                                cmd.Parameters.AddWithValue("@DeliveryCharge", deliveryCharge);
                                cmd.Parameters.AddWithValue("@DeliveryAddress", deliveryAddress ?? "");
                                cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod ?? "");
                                cmd.Parameters.Add(new SqlParameter("@PaymentMobile", SqlDbType.NVarChar, 20)
                                    { Value = (object)mobile ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@ImagePath", SqlDbType.NVarChar, 250)
                                    { Value = needsRx && storedPath != null ? storedPath : (object)DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@DoctorName", SqlDbType.NVarChar, 100)
                                    { Value = string.IsNullOrWhiteSpace(doctorName) ? DBNull.Value : (object)doctorName.Trim() });
                                newOrderId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            tx.Commit();
                            committed = true;

                            // An image picked for a slice that turned out not to
                            // need one was never recorded, so it is not kept.
                            if (!needsRx) PrescriptionService.DeleteStoredImage(storedFullPath);

                            message = "Order placed.";
                            return newOrderId;
                        }
                        catch (Exception ex)
                        {
                            try { tx.Rollback(); } catch { /* connection already gone */ }
                            message = "The order could not be placed. " + DbHelper.Describe(ex);
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Opening the connection failed; nothing was written.
                message = "The order could not be placed. " + DbHelper.Describe(ex);
                return 0;
            }
            finally
            {
                if (!committed) PrescriptionService.DeleteStoredImage(storedFullPath);
            }
        }

        /// <summary>
        /// The first reason the locked cart slice cannot become an order, or null
        /// when it can. Messages name the medicine so the customer knows which
        /// line to change.
        /// </summary>
        private static string FindCheckoutProblem(DataTable lines, bool prescriptionAttached)
        {
            if (lines.Rows.Count == 0)
                return "There is nothing left in your cart for this pharmacy. " +
                       "It may already have been ordered from another window.";

            foreach (DataRow row in lines.Rows)
            {
                string name = "'" + DbHelper.GetString(row, "MedicineName") + "'";

                if (DbHelper.GetString(row, "PharmacyStatus") != "Approved")
                    return name + " cannot be bought because the pharmacy is not taking orders at the moment. " +
                           "Please remove it from your cart.";

                if (!DbHelper.GetBool(row, "IsActive"))
                    return name + " is no longer sold by this pharmacy. Please remove it from your cart.";

                if (DbHelper.GetInt(row, "IsExpired") == 1)
                    return name + " has passed its expiry date and cannot be sold. Please remove it from your cart.";

                if (DbHelper.GetInt(row, "Quantity") > DbHelper.GetInt(row, "Stock"))
                    return name + " is no longer available in the quantity you asked for (" +
                           DbHelper.GetInt(row, "Stock") + " left). Please update your cart and try again.";
            }

            bool needsRx = lines.Rows.Cast<DataRow>().Any(r => DbHelper.GetBool(r, "RequiresRx"));
            if (needsRx && !prescriptionAttached)
                return "This order contains a prescription only medicine. " +
                       "Attach a photograph of your prescription before confirming.";

            return null;
        }

        // =====================================================================
        //  ORDER HISTORY  (requirement 27)
        // =====================================================================

        /// <summary>
        /// The customer's own orders, with the number of line items, the invoice
        /// total from the computed TotalAmount column, a CanReview flag that
        /// the Rate and Review button binds to, and the prescription state
        /// (RxState, plus RejectReason when the current one was rejected).
        /// </summary>
        public DataTable GetHistoryForCustomer(int customerId, string status, int pharmacyId,
                                               DateTime fromDate, DateTime toDate)
        {
            string sql = @"
SELECT  o.OrderId, o.OrderDate, ph.PharmacyName,
        (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS Items,
        o.ItemsTotal, o.DeliveryCharge, o.TotalAmount,
        o.PaymentMethod, o.Status," + RxStateExpression + @" AS RxState,
        rx.RejectReason,
        CASE WHEN o.Status = 'Delivered'
              AND EXISTS (SELECT 1 FROM OrderItems x
                          WHERE x.OrderId = o.OrderId
                            AND NOT EXISTS (SELECT 1 FROM Reviews r
                                            WHERE r.OrderId = o.OrderId
                                              AND r.MedicineId = x.MedicineId))
             THEN 1 ELSE 0 END     AS CanReview
FROM    Orders o
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId" + CurrentRxApply + @"
WHERE   o.CustomerId = @CustomerId
  AND   (@Status     = '' OR o.Status = @Status)
  AND   (@PharmacyId = 0  OR o.PharmacyId = @PharmacyId)
  AND   o.OrderDate BETWEEN @FromDate AND @ToDate
ORDER BY o.OrderDate DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@CustomerId", customerId),
                DbHelper.P("@Status", status ?? ""),
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@FromDate", fromDate.Date),
                DbHelper.P("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1)));
        }

        /// <summary>
        /// Every pharmacy this customer has ever ordered from, whatever that
        /// pharmacy's status is today, for the history filter. A suspended shop's
        /// old orders must still be findable.
        /// </summary>
        public DataTable GetPharmaciesForCustomer(int customerId)
        {
            return _db.ExecuteTable(@"
SELECT  DISTINCT ph.PharmacyId, ph.PharmacyName
FROM    Orders o INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
WHERE   o.CustomerId = @CustomerId
ORDER BY ph.PharmacyName;",
                DbHelper.P("@CustomerId", customerId));
        }

        /// <summary>Everything the printable invoice needs, in one object.</summary>
        public Order GetOrderWithItems(int orderId)
        {
            const string header = @"
SELECT  o.OrderId, o.CustomerId, o.PharmacyId, o.OrderDate, o.ItemsTotal,
        o.DeliveryCharge, o.TotalAmount, o.CommissionAmount, o.DeliveryAddress,
        o.PaymentMethod, o.PaymentMobile, o.Status,
        u.FullName AS CustomerName, u.Phone AS CustomerPhone,
        ph.PharmacyName, ph.Address AS PharmacyAddress, ph.LicenseNo AS PharmacyLicense
FROM    Orders o
        INNER JOIN Users u       ON u.UserId       = o.CustomerId
        INNER JOIN Pharmacies ph ON ph.PharmacyId  = o.PharmacyId
WHERE   o.OrderId = @OrderId;";

            DataTable table = _db.ExecuteTable(header, DbHelper.P("@OrderId", orderId));
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            Order order = new Order
            {
                OrderId = DbHelper.GetInt(row, "OrderId"),
                CustomerId = DbHelper.GetInt(row, "CustomerId"),
                PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                OrderDate = DbHelper.GetDate(row, "OrderDate"),
                ItemsTotal = DbHelper.GetDecimal(row, "ItemsTotal"),
                DeliveryCharge = DbHelper.GetDecimal(row, "DeliveryCharge"),
                TotalAmount = DbHelper.GetDecimal(row, "TotalAmount"),
                CommissionAmount = DbHelper.GetDecimal(row, "CommissionAmount"),
                DeliveryAddress = DbHelper.GetString(row, "DeliveryAddress"),
                PaymentMethod = DbHelper.GetString(row, "PaymentMethod"),
                PaymentMobile = DbHelper.GetString(row, "PaymentMobile"),
                Status = DbHelper.GetString(row, "Status"),
                CustomerName = DbHelper.GetString(row, "CustomerName"),
                CustomerPhone = DbHelper.GetString(row, "CustomerPhone"),
                PharmacyName = DbHelper.GetString(row, "PharmacyName"),
                PharmacyAddress = DbHelper.GetString(row, "PharmacyAddress"),
                PharmacyLicense = DbHelper.GetString(row, "PharmacyLicense")
            };

            const string lines = @"
SELECT  oi.OrderItemId, oi.OrderId, oi.MedicineId, oi.Quantity, oi.UnitPrice, oi.Subtotal,
        m.MedicineName, m.Strength
FROM    OrderItems oi
        INNER JOIN Medicines m ON m.MedicineId = oi.MedicineId
WHERE   oi.OrderId = @OrderId
ORDER BY m.MedicineName;";

            DataTable items = _db.ExecuteTable(lines, DbHelper.P("@OrderId", orderId));
            foreach (DataRow line in items.Rows)
            {
                order.Items.Add(new OrderItem
                {
                    OrderItemId = DbHelper.GetInt(line, "OrderItemId"),
                    OrderId = DbHelper.GetInt(line, "OrderId"),
                    MedicineId = DbHelper.GetInt(line, "MedicineId"),
                    Quantity = DbHelper.GetInt(line, "Quantity"),
                    UnitPrice = DbHelper.GetDecimal(line, "UnitPrice"),
                    Subtotal = DbHelper.GetDecimal(line, "Subtotal"),
                    MedicineName = DbHelper.GetString(line, "MedicineName"),
                    Strength = DbHelper.GetString(line, "Strength")
                });
            }

            return order;
        }

        /// <summary>
        /// How much today's offers took off this order, for the invoice's
        /// savings line. OrderItems keeps only the price actually paid, so the
        /// list price is reconstructed from the best offer that was running on
        /// the order date: when the medicine's current list price still produces
        /// the paid price it is used exactly, otherwise the paid price is grossed
        /// back up by that discount. A line bought at or above today's list price,
        /// or on a day with no offer, saved nothing.
        /// </summary>
        public decimal GetDiscountSavings(int orderId)
        {
            const string sql = @"
SELECT  ISNULL(SUM(oi.Quantity *
            CASE WHEN d.Pct IS NULL OR oi.UnitPrice >= m.UnitPrice THEN 0
                 WHEN CAST(ROUND(m.UnitPrice * (1 - d.Pct/100.0), 2) AS DECIMAL(10,2)) = oi.UnitPrice
                      THEN m.UnitPrice - oi.UnitPrice
                 ELSE CAST(ROUND(oi.UnitPrice / (1 - d.Pct/100.0), 2) AS DECIMAL(10,2)) - oi.UnitPrice
            END), 0)
FROM    OrderItems oi
        INNER JOIN Orders    ord ON ord.OrderId  = oi.OrderId
        INNER JOIN Medicines m   ON m.MedicineId = oi.MedicineId
        OUTER APPLY (SELECT MAX(o.DiscountPercent) AS Pct
                     FROM   Offers o
                     WHERE  o.MedicineId = oi.MedicineId
                       AND  CAST(ord.OrderDate AS DATE) BETWEEN o.StartDate AND o.EndDate) d
WHERE   oi.OrderId = @OrderId;";

            decimal savings = _db.ExecuteScalarDecimal(sql, DbHelper.P("@OrderId", orderId));
            return savings < 0m ? 0m : decimal.Round(savings, 2);
        }

        public DataTable GetOrderItems(int orderId)
        {
            const string sql = @"
SELECT  m.MedicineName, m.Strength, oi.Quantity, oi.UnitPrice, oi.Subtotal
FROM    OrderItems oi
        INNER JOIN Medicines m ON m.MedicineId = oi.MedicineId
WHERE   oi.OrderId = @OrderId
ORDER BY m.MedicineName;";

            return _db.ExecuteTable(sql, DbHelper.P("@OrderId", orderId));
        }

        // =====================================================================
        //  PHARMACY OWNER: ORDER QUEUE AND STATUS
        // =====================================================================

        /// <summary>
        /// Orders belonging to this pharmacy only. RxState is exactly one of
        /// 'Not needed', 'Missing', 'Pending', 'Approved' or 'Rejected', read
        /// from the order's current prescription; Confirm is allowed only for
        /// 'Not needed' and 'Approved'.
        /// </summary>
        public DataTable GetOrdersForPharmacy(int pharmacyId, string status)
        {
            string sql = @"
SELECT  o.OrderId, o.OrderDate, u.FullName AS CustomerName, u.Phone AS CustomerPhone,
        (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS Items,
        o.ItemsTotal, o.DeliveryCharge, o.TotalAmount,
        o.PaymentMethod, o.Status, o.DeliveryAddress," + RxStateExpression + @" AS RxState
FROM    Orders o
        INNER JOIN Users u ON u.UserId = o.CustomerId" + CurrentRxApply + @"
WHERE   o.PharmacyId = @PharmacyId
  AND   (@Status = '' OR o.Status = @Status)
ORDER BY o.OrderDate DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@Status", status ?? ""));
        }

        /// <summary>
        /// Moves a Placed order to Confirmed, but only when its RxState is
        /// 'Not needed' or 'Approved'. The rule lives in the UPDATE itself rather
        /// than trusting the form to disable a button: an order with a
        /// prescription only medicine is confirmable only when its CURRENT
        /// prescription is Approved, so a missing, pending or rejected one
        /// changes no rows.
        /// </summary>
        public bool Confirm(int orderId, int pharmacyId)
        {
            const string sql = @"
UPDATE  o
SET     o.Status = 'Confirmed'
FROM    Orders o
WHERE   o.OrderId    = @OrderId
  AND   o.PharmacyId = @PharmacyId
  AND   o.Status     = 'Placed'
  AND   (   NOT EXISTS (SELECT 1 FROM OrderItems xi
                        INNER JOIN Medicines xm ON xm.MedicineId = xi.MedicineId
                        WHERE xi.OrderId = o.OrderId AND xm.RequiresRx = 1)
         OR (SELECT TOP 1 p.VerifyStatus FROM Prescriptions p
             WHERE p.OrderId = o.OrderId ORDER BY p.PrescriptionId DESC) = 'Approved');";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        public bool MarkDelivered(int orderId, int pharmacyId)
        {
            return _db.ExecuteNonQuery(
                "UPDATE Orders SET Status = 'Delivered' WHERE OrderId = @OrderId AND PharmacyId = @PharmacyId AND Status = 'Confirmed';",
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        /// <summary>
        /// Cancelling puts the stock back on the shelf, inside one transaction.
        ///
        /// Both statements are guarded by the same condition on purpose. An
        /// earlier version restocked whenever the order was not already
        /// cancelled but only flipped the status when it was not delivered, so
        /// cancelling a delivered order returned the units to stock and left the
        /// order reading Delivered. The eligibility test is now made once, in
        /// the database, and both statements sit behind it.
        ///
        /// The test reads the order WITH (UPDLOCK, HOLDLOCK), so the row stays
        /// locked until COMMIT. Without that, two cancels clicked at once (or a
        /// cancel racing a delivery) could both pass the IF EXISTS and restock
        /// the same units twice.
        /// </summary>
        public bool Cancel(int orderId, int pharmacyId)
        {
            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Cancelled INT = 0;

    IF EXISTS (SELECT 1 FROM Orders WITH (UPDLOCK, HOLDLOCK)
               WHERE OrderId    = @OrderId
                 AND PharmacyId = @PharmacyId
                 AND Status NOT IN ('Delivered', 'Cancelled'))
    BEGIN
        UPDATE  m SET m.Stock = m.Stock + oi.Quantity
        FROM    Medicines m INNER JOIN OrderItems oi ON oi.MedicineId = m.MedicineId
        WHERE   oi.OrderId = @OrderId;

        UPDATE  Orders SET Status = 'Cancelled'
        WHERE   OrderId    = @OrderId
          AND   PharmacyId = @PharmacyId
          AND   Status NOT IN ('Delivered', 'Cancelled');

        SET @Cancelled = @@ROWCOUNT;
    END

    COMMIT TRANSACTION;
    SELECT @Cancelled;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;";

            return _db.ExecuteScalarInt(sql,
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        /// <summary>
        /// The customer's own cancel, from My Orders. Allowed only while the
        /// order is still 'Placed' - once the pharmacy has confirmed it the
        /// medicine may already be packed. Same pattern as Cancel: the order row
        /// is locked for the eligibility test, and the restock and the status
        /// change commit together or not at all. Returns false when the order is
        /// not this customer's or is no longer Placed.
        /// </summary>
        public bool CancelByCustomer(int orderId, int customerId)
        {
            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Cancelled INT = 0;

    IF EXISTS (SELECT 1 FROM Orders WITH (UPDLOCK, HOLDLOCK)
               WHERE OrderId    = @OrderId
                 AND CustomerId = @CustomerId
                 AND Status     = 'Placed')
    BEGIN
        UPDATE  m SET m.Stock = m.Stock + oi.Quantity
        FROM    Medicines m INNER JOIN OrderItems oi ON oi.MedicineId = m.MedicineId
        WHERE   oi.OrderId = @OrderId;

        UPDATE  Orders SET Status = 'Cancelled'
        WHERE   OrderId    = @OrderId
          AND   CustomerId = @CustomerId
          AND   Status     = 'Placed';

        SET @Cancelled = @@ROWCOUNT;
    END

    COMMIT TRANSACTION;
    SELECT @Cancelled;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;";

            return _db.ExecuteScalarInt(sql,
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@CustomerId", customerId)) == 1;
        }

        public bool OrderBelongsToCustomer(int orderId, int customerId)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Orders WHERE OrderId = @OrderId AND CustomerId = @CustomerId;",
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@CustomerId", customerId)) == 1;
        }
    }
}

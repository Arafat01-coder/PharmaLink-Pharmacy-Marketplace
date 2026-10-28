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

        /// <summary>
        /// The order's live delivery, as dv: the one Deliveries row that is
        /// Assigned, PickedUp or Delivered (UX_Deliveries_ActiveOrder allows at
        /// most one), with the delivery man's name and phone. Released and
        /// Cancelled rows are history and are ignored.
        /// </summary>
        private const string LiveDeliveryApply = @"
        OUTER APPLY (SELECT TOP 1 dl.Status, dm.FullName AS RiderName, dm.Phone AS RiderPhone
                     FROM   Deliveries dl
                            INNER JOIN Users dm ON dm.UserId = dl.DeliveryManId
                     WHERE  dl.OrderId = o.OrderId
                       AND  dl.Status IN ('Assigned', 'PickedUp', 'Delivered')) dv";

        /// <summary>
        /// One readable sentence for the Delivery column. Expects o and dv.
        /// An order delivered without a Deliveries row was taken to the door by
        /// the pharmacy itself (or was delivered before delivery men existed).
        /// </summary>
        private const string DeliveryTextExpression = @"
        CASE WHEN dv.Status = 'Assigned'  THEN 'Accepted by ' + dv.RiderName
             WHEN dv.Status = 'PickedUp'  THEN 'On the way: ' + dv.RiderName + ', ' + dv.RiderPhone
             WHEN dv.Status = 'Delivered' THEN 'Delivered by ' + dv.RiderName
             WHEN o.Status  = 'Confirmed' THEN 'Waiting for a delivery man'
             WHEN o.Status  = 'Delivered' THEN 'Delivered by the pharmacy'
             ELSE '-' END";

        // =====================================================================
        //  CHECKOUT  (requirements 25 and 26)
        // =====================================================================

        /// <summary>
        /// Places one order for one pharmacy's slice of the basket. A cart that
        /// spans two pharmacies is checked out by calling this once per
        /// pharmacy, which is why every statement below carries @PharmacyId.
        ///
        /// <paramref name="paymentMobile"/> is the bKash or Nagad wallet number
        /// and is stored only for those two methods.
        ///
        /// <paramref name="payment"/> is the gateway's approval for bKash, Nagad
        /// or Card, and must be null for cash on delivery. The gateway approved
        /// the amount the customer was shown; the batch below checks it against
        /// the total it works out under lock and refuses the order if the two
        /// differ, so a price that changed in the meantime is never charged.
        /// The Payments row is written in the same transaction as the order:
        /// Paid with the gateway reference for an online payment, Pending for
        /// cash. If the order is not placed, nothing records the approval and
        /// the customer is told nothing was charged. When the slice contains a
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
                            PaymentAuthorization payment,
                            string prescriptionImagePath, string doctorName, out string message)
        {
            bool walletPayment = paymentMethod == "bKash" || paymentMethod == "Nagad";
            bool onlinePayment = walletPayment || paymentMethod == "Card";

            // The wallet that paid is the one the gateway verified.
            if (walletPayment && payment != null && !string.IsNullOrWhiteSpace(payment.WalletNumber))
                paymentMobile = payment.WalletNumber;
            string mobile = walletPayment && !string.IsNullOrWhiteSpace(paymentMobile) ? paymentMobile.Trim() : null;

            if (walletPayment && mobile == null)
            {
                message = "Enter the " + paymentMethod + " number the payment will come from.";
                return 0;
            }
            if (onlinePayment && (payment == null || payment.Method != paymentMethod))
            {
                message = "The " + paymentMethod + " payment has not been completed. Nothing was charged.";
                return 0;
            }
            if (!onlinePayment) payment = null;

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
                            // HOLDLOCK on Cart locks the customer's key range, not just
                            // the rows that exist now, so a line added from a second
                            // window cannot slip into this order unchecked.
                            const string lockAndCheck = @"
SELECT  m.MedicineId, m.MedicineName, ct.Quantity, m.Stock, m.IsActive, m.RequiresRx,
        CASE WHEN m.ExpiryDate > CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END AS IsExpired,
        ph.Status AS PharmacyStatus
FROM    Cart ct WITH (UPDLOCK, HOLDLOCK)
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

-- The lines about to become the order must be exactly the lines that were
-- locked and checked above; anything else means the cart changed underneath.
IF (SELECT COUNT(*) FROM @Lines) <> @CheckedLines
    THROW 50001, 'Your cart changed while the order was being placed. Please review your cart and try again.', 1;

-- Whether a prescription is needed is decided from these lines, not from
-- what the screen believed.
IF @ImagePath IS NULL AND EXISTS (SELECT 1 FROM @Lines l
                                  INNER JOIN Medicines m ON m.MedicineId = l.MedicineId
                                  WHERE m.RequiresRx = 1)
    THROW 50002, 'This order contains a prescription-only medicine. Attach a photo of the prescription and try again.', 1;

DECLARE @Total DECIMAL(12,2) = (SELECT CAST(SUM(Quantity * UnitPrice) AS DECIMAL(12,2)) FROM @Lines);
DECLARE @CommRate DECIMAL(5,2) = (SELECT CommissionRate FROM Pharmacies WHERE PharmacyId = @PharmacyId);

-- An online payment was approved for the amount on screen; it must be the
-- amount this order really comes to.
IF @AuthorizedAmount IS NOT NULL AND @AuthorizedAmount <> @Total + @DeliveryCharge
    THROW 50003, 'The order total changed while you were paying, so the payment was not taken. Please check the new total and pay again.', 1;

INSERT INTO Orders (CustomerId, PharmacyId, ItemsTotal, DeliveryCharge, CommissionAmount,
                    DeliveryAddress, PaymentMethod, PaymentMobile, Status)
VALUES (@CustomerId, @PharmacyId, @Total, @DeliveryCharge,
        CAST(ROUND(@Total * @CommRate / 100.0, 2) AS DECIMAL(12,2)),
        @DeliveryAddress, @PaymentMethod, @PaymentMobile, 'Placed');

DECLARE @NewOrderId INT = CAST(SCOPE_IDENTITY() AS INT);

-- one line per cart row, at the discounted price the customer actually saw
INSERT INTO OrderItems (OrderId, MedicineId, Quantity, UnitPrice)
SELECT  @NewOrderId, MedicineId, Quantity, UnitPrice FROM @Lines;

-- the payment: Paid with the gateway reference, or Pending until the cash is collected
INSERT INTO Payments (OrderId, Status, TransactionRef, CardLast4, PaidAt)
VALUES (@NewOrderId,
        CASE WHEN @TransactionRef IS NULL THEN 'Pending' ELSE 'Paid' END,
        @TransactionRef, @CardLast4,
        CASE WHEN @TransactionRef IS NULL THEN NULL ELSE SYSDATETIME() END);

-- the prescription is part of the order, so it commits or rolls back with it
IF @ImagePath IS NOT NULL AND EXISTS (SELECT 1 FROM @Lines l
                                      INNER JOIN Medicines m ON m.MedicineId = l.MedicineId
                                      WHERE m.RequiresRx = 1)
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
                                cmd.Parameters.AddWithValue("@CheckedLines", lines.Rows.Count);
                                cmd.Parameters.Add(new SqlParameter("@PaymentMobile", SqlDbType.NVarChar, 20)
                                    { Value = (object)mobile ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@ImagePath", SqlDbType.NVarChar, 250)
                                    { Value = storedPath != null ? storedPath : (object)DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@DoctorName", SqlDbType.NVarChar, 100)
                                    { Value = string.IsNullOrWhiteSpace(doctorName) ? DBNull.Value : (object)doctorName.Trim() });
                                cmd.Parameters.Add(new SqlParameter("@AuthorizedAmount", SqlDbType.Decimal)
                                    { Precision = 12, Scale = 2, Value = payment != null ? payment.Amount : (object)DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@TransactionRef", SqlDbType.NVarChar, 40)
                                    { Value = payment != null ? payment.TransactionRef : (object)DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@CardLast4", SqlDbType.Char, 4)
                                    { Value = payment != null && payment.CardLast4.Length == 4 ? payment.CardLast4 : (object)DBNull.Value });
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

                            // 50001 to 50003 are this batch's own THROWs and are
                            // already written for the customer.
                            message = ex is SqlException own && own.Number >= 50000
                                ? own.Message
                                : "The order could not be placed. " + DbHelper.Describe(ex);
                            if (payment != null)
                                message += "\r\n\r\nNo money has been taken for this order.";
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Opening the connection failed; nothing was written.
                message = "The order could not be placed. " + DbHelper.Describe(ex);
                if (payment != null)
                    message += "\r\n\r\nNo money has been taken for this order.";
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
        /// the Rate and Review button binds to, the prescription state
        /// (RxState, plus RejectReason when the current one was rejected), the
        /// payment status and who is delivering the order.
        /// </summary>
        public DataTable GetHistoryForCustomer(int customerId, string status, int pharmacyId,
                                               DateTime fromDate, DateTime toDate)
        {
            string sql = @"
SELECT  o.OrderId, o.OrderDate, ph.PharmacyName,
        (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS Items,
        o.ItemsTotal, o.DeliveryCharge, o.TotalAmount,
        o.PaymentMethod, ISNULL(pay.Status, '-') AS PaymentStatus,
        o.Status," + DeliveryTextExpression + @" AS Delivery," + RxStateExpression + @" AS RxState,
        rx.RejectReason,
        CASE WHEN o.Status = 'Delivered'
              AND EXISTS (SELECT 1 FROM OrderItems x
                          WHERE x.OrderId = o.OrderId
                            AND NOT EXISTS (SELECT 1 FROM Reviews r
                                            WHERE r.OrderId = o.OrderId
                                              AND r.MedicineId = x.MedicineId))
             THEN 1 ELSE 0 END     AS CanReview
FROM    Orders o
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
        LEFT  JOIN Payments  pay ON pay.OrderId   = o.OrderId" + CurrentRxApply + LiveDeliveryApply + @"
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
            string header = @"
SELECT  o.OrderId, o.CustomerId, o.PharmacyId, o.OrderDate, o.ItemsTotal,
        o.DeliveryCharge, o.TotalAmount, o.CommissionAmount, o.DeliveryAddress,
        o.PaymentMethod, o.PaymentMobile, o.Status,
        ISNULL(pay.Status, '') AS PaymentStatus, pay.TransactionRef, pay.CardLast4, pay.PaidAt,
        dv.Status AS DeliveryStatus, dv.RiderName, dv.RiderPhone,
        u.FullName AS CustomerName, u.Phone AS CustomerPhone,
        ph.PharmacyName, ph.Address AS PharmacyAddress, ph.LicenseNo AS PharmacyLicense
FROM    Orders o
        INNER JOIN Users u       ON u.UserId       = o.CustomerId
        INNER JOIN Pharmacies ph ON ph.PharmacyId  = o.PharmacyId
        LEFT  JOIN Payments  pay ON pay.OrderId    = o.OrderId" + LiveDeliveryApply + @"
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
                PaymentStatus = DbHelper.GetString(row, "PaymentStatus"),
                TransactionRef = DbHelper.GetString(row, "TransactionRef"),
                CardLast4 = DbHelper.GetString(row, "CardLast4"),
                PaidAt = row["PaidAt"] == DBNull.Value ? (DateTime?)null : DbHelper.GetDate(row, "PaidAt"),
                DeliveryStatus = DbHelper.GetString(row, "DeliveryStatus"),
                DeliveryManName = DbHelper.GetString(row, "RiderName"),
                DeliveryManPhone = DbHelper.GetString(row, "RiderPhone"),
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
        /// 'Not needed' and 'Approved'. DeliveryStatus is the raw status of the
        /// live delivery (empty when no delivery man has the order) and drives
        /// the Mark delivered and Cancel buttons; Delivery is the same thing as
        /// a sentence for the grid.
        /// </summary>
        public DataTable GetOrdersForPharmacy(int pharmacyId, string status)
        {
            string sql = @"
SELECT  o.OrderId, o.OrderDate, u.FullName AS CustomerName, u.Phone AS CustomerPhone,
        (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS Items,
        o.ItemsTotal, o.DeliveryCharge, o.TotalAmount,
        o.PaymentMethod, ISNULL(pay.Status, '-') AS PaymentStatus,
        o.Status, o.DeliveryAddress," + RxStateExpression + @" AS RxState,
        ISNULL(dv.Status, '') AS DeliveryStatus," + DeliveryTextExpression + @" AS Delivery
FROM    Orders o
        INNER JOIN Users u     ON u.UserId     = o.CustomerId
        LEFT  JOIN Payments pay ON pay.OrderId = o.OrderId" + CurrentRxApply + LiveDeliveryApply + @"
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

        /// <summary>
        /// The pharmacy delivering an order itself. Not allowed once a delivery
        /// man has accepted the order - then it is his job to mark it delivered,
        /// at the customer's door. A cash order delivered by the pharmacy is
        /// marked Paid in the same transaction, with no delivery man recorded as
        /// the collector, because the pharmacy took the cash.
        /// </summary>
        public bool MarkDelivered(int orderId, int pharmacyId)
        {
            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE  o SET o.Status = 'Delivered'
    FROM    Orders o
    WHERE   o.OrderId    = @OrderId
      AND   o.PharmacyId = @PharmacyId
      AND   o.Status     = 'Confirmed'
      AND   NOT EXISTS (SELECT 1 FROM Deliveries d
                        WHERE d.OrderId = o.OrderId
                          AND d.Status IN ('Assigned', 'PickedUp', 'Delivered'));

    DECLARE @Delivered INT = @@ROWCOUNT;

    IF @Delivered = 1
        UPDATE Payments SET Status = 'Paid', PaidAt = SYSDATETIME()
        WHERE  OrderId = @OrderId AND Status = 'Pending';

    COMMIT TRANSACTION;
    SELECT @Delivered;
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
        /// What cancelling does to the money and to the delivery, shared by
        /// both cancel paths. Expects @OrderId. An online payment is refunded,
        /// a cash payment that was never collected is cancelled, and a delivery
        /// man who had accepted but not yet picked up the order is released.
        /// </summary>
        private const string CancelSideEffects = @"
        UPDATE  Payments
        SET     Status     = CASE WHEN Status = 'Paid' THEN 'Refunded' ELSE 'Cancelled' END,
                RefundedAt = CASE WHEN Status = 'Paid' THEN SYSDATETIME() ELSE RefundedAt END
        WHERE   OrderId = @OrderId AND Status IN ('Paid', 'Pending');

        UPDATE  Deliveries SET Status = 'Cancelled', ReleasedAt = SYSDATETIME()
        WHERE   OrderId = @OrderId AND Status = 'Assigned';";

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
        ///
        /// Once a delivery man has picked the order up the medicine has left the
        /// shop, so it can no longer be cancelled here. Before pickup, the
        /// delivery man is released, and the payment is refunded or cancelled
        /// (see <see cref="CancelSideEffects"/>).
        /// </summary>
        public bool Cancel(int orderId, int pharmacyId)
        {
            string sql = @"
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Cancelled INT = 0;

    IF EXISTS (SELECT 1 FROM Orders o WITH (UPDLOCK, HOLDLOCK)
               WHERE o.OrderId    = @OrderId
                 AND o.PharmacyId = @PharmacyId
                 AND o.Status NOT IN ('Delivered', 'Cancelled')
                 AND NOT EXISTS (SELECT 1 FROM Deliveries d WITH (UPDLOCK, HOLDLOCK)
                                 WHERE d.OrderId = o.OrderId AND d.Status IN ('PickedUp', 'Delivered')))
    BEGIN
        UPDATE  m SET m.Stock = m.Stock + oi.Quantity
        FROM    Medicines m INNER JOIN OrderItems oi ON oi.MedicineId = m.MedicineId
        WHERE   oi.OrderId = @OrderId;

        UPDATE  Orders SET Status = 'Cancelled'
        WHERE   OrderId    = @OrderId
          AND   PharmacyId = @PharmacyId
          AND   Status NOT IN ('Delivered', 'Cancelled');

        SET @Cancelled = @@ROWCOUNT;
" + CancelSideEffects + @"
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
        /// not this customer's or is no longer Placed. A Placed order never has
        /// a delivery man yet; an online payment on it is refunded.
        /// </summary>
        public bool CancelByCustomer(int orderId, int customerId)
        {
            string sql = @"
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
" + CancelSideEffects + @"
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

        /// <summary>
        /// The lines of one of the customer's own past orders, with the
        /// MedicineId that "Reorder" needs to put each one back in the cart.
        ///
        /// The join to Orders carries CustomerId, so an order number that is
        /// not this customer's returns no rows rather than someone else's
        /// shopping list. Prices are deliberately not returned: a reorder is
        /// charged at today's price, which the cart and checkout work out.
        /// </summary>
        public DataTable GetReorderLines(int orderId, int customerId)
        {
            const string sql = @"
SELECT  oi.MedicineId, m.MedicineName, m.Strength, oi.Quantity
FROM    OrderItems oi
        INNER JOIN Orders    o ON o.OrderId    = oi.OrderId
        INNER JOIN Medicines m ON m.MedicineId = oi.MedicineId
WHERE   oi.OrderId   = @OrderId
  AND   o.CustomerId = @CustomerId
ORDER BY m.MedicineName;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@CustomerId", customerId));
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

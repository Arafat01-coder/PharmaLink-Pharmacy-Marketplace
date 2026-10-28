using System.Data;
using PharmaLinkApp.Database;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Everything a delivery man does, and the rules around it.
    ///
    /// The life of a delivery:
    ///   1. A pharmacy confirms an order. It now sits in the pool, which every
    ///      active delivery man sees on his Available tab.
    ///   2. One delivery man accepts it (Deliveries row, Status 'Assigned').
    ///      UX_Deliveries_ActiveOrder allows only one live row per order, so
    ///      if two accept the same order at the same moment the second insert
    ///      fails and he is told someone was quicker.
    ///   3. He collects it from the pharmacy ('PickedUp'). Until then he may
    ///      give the job back ('Released'), and it returns to the pool.
    ///   4. He hands it to the customer ('Delivered'). In one transaction this
    ///      also moves Orders.Status to 'Delivered' and, for cash on delivery,
    ///      marks the payment Paid with him as the collector.
    ///
    /// Every statement carries @DeliveryManId, so one delivery man can never
    /// move another's delivery, in the same way every pharmacy statement
    /// carries @PharmacyId.
    /// </summary>
    public class DeliveryService
    {
        private readonly DbHelper _db = new DbHelper();

        /// <summary>
        /// Confirmed orders that nobody has accepted, from approved pharmacies.
        /// CashToCollect is the full bill for cash on delivery and zero for an
        /// order that was paid online, so the delivery man knows before he
        /// accepts whether he will be handling money.
        /// </summary>
        public DataTable GetAvailableOrders()
        {
            const string sql = @"
SELECT  o.OrderId, o.OrderDate,
        ph.PharmacyName, ph.Area AS PickupArea, ph.Address AS PickupAddress,
        o.DeliveryAddress,
        (SELECT SUM(oi.Quantity) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS Units,
        CASE WHEN o.PaymentMethod = 'CashOnDelivery' THEN o.TotalAmount ELSE 0 END AS CashToCollect,
        o.DeliveryCharge AS DeliveryFee
FROM    Orders o
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
WHERE   o.Status  = 'Confirmed'
  AND   ph.Status = 'Approved'
  AND   NOT EXISTS (SELECT 1 FROM Deliveries d
                    WHERE d.OrderId = o.OrderId
                      AND d.Status IN ('Assigned', 'PickedUp', 'Delivered'))
ORDER BY o.OrderDate;";

            return _db.ExecuteTable(sql);
        }

        /// <summary>The deliveries this man has accepted and not finished yet.</summary>
        public DataTable GetMyActiveDeliveries(int deliveryManId)
        {
            const string sql = @"
SELECT  o.OrderId, d.Status, d.AcceptedAt, d.PickedUpAt,
        ph.PharmacyName, ph.Address AS PickupAddress, ph.ContactPhone AS PharmacyPhone,
        u.FullName AS CustomerName, u.Phone AS CustomerPhone, o.DeliveryAddress,
        CASE WHEN o.PaymentMethod = 'CashOnDelivery' THEN o.TotalAmount ELSE 0 END AS CashToCollect,
        o.PaymentMethod
FROM    Deliveries d
        INNER JOIN Orders     o  ON o.OrderId     = d.OrderId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
        INNER JOIN Users      u  ON u.UserId      = o.CustomerId
WHERE   d.DeliveryManId = @DeliveryManId
  AND   d.Status IN ('Assigned', 'PickedUp')
ORDER BY d.AcceptedAt;";

            return _db.ExecuteTable(sql, DbHelper.P("@DeliveryManId", deliveryManId));
        }

        /// <summary>
        /// Finished jobs between two dates: delivered, given back, or cancelled
        /// by the pharmacy while assigned. CashCollected is what he took at the
        /// door; the delivery fee is what the delivery earned.
        /// </summary>
        public DataTable GetHistory(int deliveryManId, DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
SELECT  o.OrderId, d.Status,
        COALESCE(d.DeliveredAt, d.ReleasedAt, d.AcceptedAt) AS FinishedAt,
        ph.PharmacyName, o.DeliveryAddress,
        CASE WHEN d.Status = 'Delivered' AND p.CollectedById = d.DeliveryManId
             THEN o.TotalAmount ELSE 0 END AS CashCollected,
        CASE WHEN d.Status = 'Delivered' THEN o.DeliveryCharge ELSE 0 END AS DeliveryFee
FROM    Deliveries d
        INNER JOIN Orders     o  ON o.OrderId     = d.OrderId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
        LEFT  JOIN Payments   p  ON p.OrderId     = o.OrderId
WHERE   d.DeliveryManId = @DeliveryManId
  AND   d.Status IN ('Delivered', 'Released', 'Cancelled')
  AND   COALESCE(d.DeliveredAt, d.ReleasedAt, d.AcceptedAt) BETWEEN @FromDate AND @ToDate
ORDER BY FinishedAt DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@DeliveryManId", deliveryManId),
                DbHelper.P("@FromDate", fromDate.Date),
                DbHelper.P("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1)));
        }

        /// <summary>
        /// The four dashboard tiles in one round trip: jobs in hand, deliveries
        /// finished today, cash collected today, and delivery fees earned this
        /// calendar month.
        /// </summary>
        public DataRow GetSummary(int deliveryManId)
        {
            const string sql = @"
DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

SELECT
    (SELECT COUNT(*) FROM Deliveries
     WHERE DeliveryManId = @DeliveryManId AND Status IN ('Assigned', 'PickedUp'))       AS InHand,
    (SELECT COUNT(*) FROM Deliveries
     WHERE DeliveryManId = @DeliveryManId AND Status = 'Delivered'
       AND CAST(DeliveredAt AS DATE) = @Today)                                            AS DeliveredToday,
    (SELECT ISNULL(SUM(o.TotalAmount), 0)
     FROM   Deliveries d
            INNER JOIN Orders   o ON o.OrderId = d.OrderId
            INNER JOIN Payments p ON p.OrderId = d.OrderId
     WHERE  d.DeliveryManId = @DeliveryManId AND d.Status = 'Delivered'
       AND  p.CollectedById = @DeliveryManId
       AND  CAST(d.DeliveredAt AS DATE) = @Today)                                         AS CashToday,
    (SELECT ISNULL(SUM(o.DeliveryCharge), 0)
     FROM   Deliveries d INNER JOIN Orders o ON o.OrderId = d.OrderId
     WHERE  d.DeliveryManId = @DeliveryManId AND d.Status = 'Delivered'
       AND  d.DeliveredAt >= @MonthStart)                                                 AS FeesThisMonth;";

            return _db.ExecuteTable(sql, DbHelper.P("@DeliveryManId", deliveryManId)).Rows[0];
        }

        /// <summary>
        /// Takes an order from the pool. Returns false, with the reason, when
        /// the order is no longer available - already taken by someone else,
        /// cancelled, or its pharmacy suspended since the list was loaded.
        /// The insert itself re-checks every condition, and the filtered unique
        /// index settles a tie between two delivery men.
        /// </summary>
        public bool Accept(int orderId, int deliveryManId, out string message)
        {
            const string sql = @"
INSERT INTO Deliveries (OrderId, DeliveryManId, Status)
SELECT  o.OrderId, @DeliveryManId, 'Assigned'
FROM    Orders o
        INNER JOIN Pharmacies ph ON ph.PharmacyId = o.PharmacyId
        INNER JOIN Users      dm ON dm.UserId     = @DeliveryManId
WHERE   o.OrderId   = @OrderId
  AND   o.Status    = 'Confirmed'
  AND   ph.Status   = 'Approved'
  AND   dm.UserType = 'DeliveryMan'
  AND   dm.Status   = 'Active'
  AND   NOT EXISTS (SELECT 1 FROM Deliveries d WITH (UPDLOCK, HOLDLOCK)
                    WHERE d.OrderId = o.OrderId
                      AND d.Status IN ('Assigned', 'PickedUp', 'Delivered'));";

            try
            {
                int rows = _db.ExecuteNonQuery(sql,
                    DbHelper.P("@OrderId", orderId),
                    DbHelper.P("@DeliveryManId", deliveryManId));

                if (rows == 1)
                {
                    message = "Order " + orderId + " is yours. Collect it from the pharmacy and press 'Picked up'.";
                    return true;
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // UX_Deliveries_ActiveOrder: another delivery man got there first.
            }

            message = "Order " + orderId + " is no longer available. Another delivery man may have accepted it, " +
                      "or it was cancelled. The list has been refreshed.";
            return false;
        }

        /// <summary>
        /// Gives an accepted job back before pickup. The order returns to the
        /// pool for someone else. Not possible once the medicine is in hand.
        /// </summary>
        public bool Release(int orderId, int deliveryManId)
        {
            return _db.ExecuteNonQuery(@"
UPDATE  Deliveries SET Status = 'Released', ReleasedAt = SYSDATETIME()
WHERE   OrderId = @OrderId AND DeliveryManId = @DeliveryManId AND Status = 'Assigned';",
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@DeliveryManId", deliveryManId)) == 1;
        }

        /// <summary>
        /// Records that the medicine has left the pharmacy with him. Refused if
        /// the order was cancelled in the meantime: a cancelled order's
        /// delivery row is no longer 'Assigned'.
        /// </summary>
        public bool MarkPickedUp(int orderId, int deliveryManId)
        {
            return _db.ExecuteNonQuery(@"
UPDATE  d SET d.Status = 'PickedUp', d.PickedUpAt = SYSDATETIME()
FROM    Deliveries d
        INNER JOIN Orders o ON o.OrderId = d.OrderId
WHERE   d.OrderId = @OrderId AND d.DeliveryManId = @DeliveryManId
  AND   d.Status  = 'Assigned'
  AND   o.Status  = 'Confirmed';",
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@DeliveryManId", deliveryManId)) == 1;
        }

        /// <summary>
        /// Hands the order over at the door. The delivery row, the order status
        /// and, for cash on delivery, the payment all change in one transaction,
        /// so the customer's history, the pharmacy's earnings and the delivery
        /// man's cash total can never disagree about whether it happened.
        /// </summary>
        public bool MarkDelivered(int orderId, int deliveryManId)
        {
            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE  d SET d.Status = 'Delivered', d.DeliveredAt = SYSDATETIME()
    FROM    Deliveries d WITH (UPDLOCK, HOLDLOCK)
            INNER JOIN Orders o WITH (UPDLOCK, HOLDLOCK) ON o.OrderId = d.OrderId
    WHERE   d.OrderId = @OrderId AND d.DeliveryManId = @DeliveryManId
      AND   d.Status  = 'PickedUp'
      AND   o.Status  = 'Confirmed';

    DECLARE @Done INT = @@ROWCOUNT;

    IF @Done = 1
    BEGIN
        UPDATE Orders SET Status = 'Delivered' WHERE OrderId = @OrderId AND Status = 'Confirmed';

        -- cash on delivery: the money is now in the delivery man's hands
        UPDATE  p SET p.Status = 'Paid', p.PaidAt = SYSDATETIME(), p.CollectedById = @DeliveryManId
        FROM    Payments p INNER JOIN Orders o ON o.OrderId = p.OrderId
        WHERE   p.OrderId = @OrderId AND p.Status = 'Pending' AND o.PaymentMethod = 'CashOnDelivery';
    END

    COMMIT TRANSACTION;
    SELECT @Done;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;";

            return _db.ExecuteScalarInt(sql,
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@DeliveryManId", deliveryManId)) == 1;
        }
    }
}

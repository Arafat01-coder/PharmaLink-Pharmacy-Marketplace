using System.Data;
using PharmaLinkApp.Database;
using PharmaLinkApp.Models;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// The customer's live basket (requirement 24).
    ///
    /// Every price the cart shows is calculated by the query with today's offer
    /// already applied, so the cart, the medicine details screen and the invoice
    /// can never disagree about what an item costs. The discounted unit price is
    /// rounded to paisa FIRST and only then multiplied by the quantity - exactly
    /// the rule OrderService.Checkout writes into OrderItems - so a line total
    /// here is the same figure the invoice will print.
    /// </summary>
    public class CartService
    {
        private readonly DbHelper _db = new DbHelper();

        /// <summary>Today's best discount for m, as d.Pct. Shared by every price query below.</summary>
        private const string BestOfferApply = @"
        OUTER APPLY (SELECT MAX(o.DiscountPercent) AS Pct
                     FROM   Offers o
                     WHERE  o.MedicineId = m.MedicineId
                       AND  o.IsActive   = 1
                       AND  CAST(GETDATE() AS DATE) BETWEEN o.StartDate AND o.EndDate) d";

        /// <summary>The discounted unit price, rounded to paisa.</summary>
        private const string UnitPriceExpression =
            "CAST(ROUND(m.UnitPrice * (1 - ISNULL(d.Pct,0)/100.0), 2) AS DECIMAL(10,2))";

        /// <summary>
        /// 1 when a customer may buy m right now: listed, not expired and sold by
        /// an Approved pharmacy (ph). Suspending a pharmacy no longer delists its
        /// medicines, so the pharmacy status has to be checked on every buy path.
        /// </summary>
        private const string CanBuyExpression =
            "CASE WHEN m.IsActive = 1 AND ph.Status = 'Approved' AND m.ExpiryDate > CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END";

        /// <summary>
        /// Why a medicine cannot go into a basket, or null when it can. Also
        /// returns the current stock, which is 0 for an unknown medicine.
        /// </summary>
        private string CheckBuyable(int medicineId, out int stock)
        {
            stock = 0;

            DataTable table = _db.ExecuteTable(@"
SELECT  m.Stock, m.IsActive, ph.Status AS PharmacyStatus,
        CASE WHEN m.ExpiryDate > CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END AS IsExpired
FROM    Medicines m INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   m.MedicineId = @Id;",
                DbHelper.P("@Id", medicineId));

            if (table.Rows.Count == 0) return "That medicine could not be found.";

            DataRow row = table.Rows[0];
            stock = DbHelper.GetInt(row, "Stock");

            if (!DbHelper.GetBool(row, "IsActive"))
                return "This medicine is no longer sold by the pharmacy.";
            if (DbHelper.GetString(row, "PharmacyStatus") != "Approved")
                return "This pharmacy is not taking orders at the moment.";
            if (DbHelper.GetInt(row, "IsExpired") == 1)
                return "This medicine has passed its expiry date and cannot be sold.";

            return null;
        }

        /// <summary>
        /// Adding a medicine that is already in the basket increases the
        /// quantity instead of creating a duplicate line. MERGE does both cases
        /// in one statement, which is what keeps the UNIQUE constraint on
        /// (CustomerId, MedicineId) from ever being violated.
        ///
        /// A delisted, expired or non-Approved-pharmacy medicine is refused with
        /// a reason; the checkout checks all of this again under lock.
        /// </summary>
        public bool AddOrIncrease(int customerId, int medicineId, int quantity, out string message)
        {
            if (quantity <= 0)
            {
                message = "Enter a quantity of one or more.";
                return false;
            }

            int stock;
            string problem = CheckBuyable(medicineId, out stock);
            if (problem != null)
            {
                message = problem;
                return false;
            }

            int alreadyInCart = _db.ExecuteScalarInt(
                "SELECT ISNULL(Quantity, 0) FROM Cart WHERE CustomerId = @Cust AND MedicineId = @Med;",
                DbHelper.P("@Cust", customerId),
                DbHelper.P("@Med", medicineId));

            if (stock <= 0)
            {
                message = "This medicine is out of stock.";
                return false;
            }

            if (alreadyInCart + quantity > stock)
            {
                message = "Only " + stock + " unit(s) are available and you already have " +
                          alreadyInCart + " in your cart.";
                return false;
            }

            const string sql = @"
MERGE Cart AS target
USING (SELECT @CustomerId AS CustomerId, @MedicineId AS MedicineId, @Quantity AS Quantity) AS source
    ON  target.CustomerId = source.CustomerId
    AND target.MedicineId = source.MedicineId
WHEN MATCHED THEN
    UPDATE SET target.Quantity = target.Quantity + source.Quantity
WHEN NOT MATCHED THEN
    INSERT (CustomerId, MedicineId, Quantity)
    VALUES (source.CustomerId, source.MedicineId, source.Quantity);";

            int rows = _db.ExecuteNonQuery(sql,
                DbHelper.P("@CustomerId", customerId),
                DbHelper.P("@MedicineId", medicineId),
                DbHelper.P("@Quantity", quantity));

            if (rows != 1)
            {
                message = "Your cart was not changed. Please try again.";
                return false;
            }

            message = "Added to cart.";
            return true;
        }

        /// <summary>
        /// Sets a line's quantity. Zero removes the line on purpose (the cart
        /// screen says so); a negative number is a mistake and is refused rather
        /// than silently treated as "remove". Any positive quantity is checked
        /// against stock and buyability, and false is returned when the line is
        /// no longer in the cart.
        /// </summary>
        public bool SetQuantity(int customerId, int medicineId, int quantity, out string message)
        {
            if (quantity < 0)
            {
                message = "The quantity cannot be negative. Enter 0 to remove the line.";
                return false;
            }

            if (quantity == 0)
            {
                if (Remove(customerId, medicineId))
                {
                    message = "Line removed from the cart.";
                    return true;
                }
                message = "That line is no longer in your cart.";
                return false;
            }

            int stock;
            string problem = CheckBuyable(medicineId, out stock);
            if (problem != null)
            {
                message = problem + " Remove it from your cart.";
                return false;
            }

            if (quantity > stock)
            {
                message = "Only " + stock + " unit(s) are in stock.";
                return false;
            }

            int rows = _db.ExecuteNonQuery(
                "UPDATE Cart SET Quantity = @Qty WHERE CustomerId = @Cust AND MedicineId = @Med;",
                DbHelper.P("@Qty", quantity),
                DbHelper.P("@Cust", customerId),
                DbHelper.P("@Med", medicineId));

            if (rows != 1)
            {
                message = "That line is no longer in your cart.";
                return false;
            }

            message = "Quantity updated.";
            return true;
        }

        public bool Remove(int customerId, int medicineId)
        {
            return _db.ExecuteNonQuery(
                "DELETE FROM Cart WHERE CustomerId = @Cust AND MedicineId = @Med;",
                DbHelper.P("@Cust", customerId),
                DbHelper.P("@Med", medicineId)) == 1;
        }

        public void ClearAll(int customerId)
        {
            _db.ExecuteNonQuery("DELETE FROM Cart WHERE CustomerId = @Cust;",
                DbHelper.P("@Cust", customerId));
        }

        public int CountLines(int customerId)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Cart WHERE CustomerId = @Cust;",
                DbHelper.P("@Cust", customerId));
        }

        /// <summary>
        /// Every basket line, with today's discount applied by the query.
        /// CanBuy is 0 for a line that the checkout would refuse because the
        /// medicine is delisted, expired or its pharmacy is not Approved.
        /// </summary>
        public DataTable GetLinesTable(int customerId)
        {
            string sql = @"
SELECT  ct.CartId,
        ct.MedicineId,
        m.MedicineName,
        m.Strength,
        ph.PharmacyId,
        ph.PharmacyName,
        ct.Quantity,
        m.UnitPrice                                                          AS ListPrice,
        ISNULL(d.Pct, 0)                                                     AS DiscountPercent,
        " + UnitPriceExpression + @"                                         AS PriceYouPay,
        CAST(ct.Quantity * " + UnitPriceExpression + @" AS DECIMAL(12,2))    AS LineTotal,
        m.Stock,
        m.RequiresRx,
        " + CanBuyExpression + @"                                            AS CanBuy
FROM    Cart ct
        INNER JOIN Medicines  m  ON m.MedicineId  = ct.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId" + BestOfferApply + @"
WHERE   ct.CustomerId = @CustomerId
ORDER BY ph.PharmacyName, m.MedicineName;";

            return _db.ExecuteTable(sql, DbHelper.P("@CustomerId", customerId));
        }

        public List<CartLine> GetLines(int customerId)
        {
            List<CartLine> lines = new List<CartLine>();
            DataTable table = GetLinesTable(customerId);

            foreach (DataRow row in table.Rows)
            {
                lines.Add(new CartLine
                {
                    CartId = DbHelper.GetInt(row, "CartId"),
                    CustomerId = customerId,
                    MedicineId = DbHelper.GetInt(row, "MedicineId"),
                    MedicineName = DbHelper.GetString(row, "MedicineName"),
                    Strength = DbHelper.GetString(row, "Strength"),
                    PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                    PharmacyName = DbHelper.GetString(row, "PharmacyName"),
                    Quantity = DbHelper.GetInt(row, "Quantity"),
                    ListPrice = DbHelper.GetDecimal(row, "ListPrice"),
                    DiscountPercent = DbHelper.GetDecimal(row, "DiscountPercent"),
                    PriceYouPay = DbHelper.GetDecimal(row, "PriceYouPay"),
                    Stock = DbHelper.GetInt(row, "Stock"),
                    RequiresRx = DbHelper.GetBool(row, "RequiresRx")
                });
            }
            return lines;
        }

        /// <summary>
        /// The basket grouped by pharmacy. This is exactly how many orders the
        /// checkout will create, because a cart that spans two pharmacies
        /// becomes two orders, each with its own delivery charge.
        ///
        /// It is also the ONE source of the cart's money figures: the cart
        /// summary adds up ItemsTotal and BeforeDiscount from these rows instead
        /// of running separate total and discount queries, so the items total,
        /// the discount and the payable amount cannot disagree with each other.
        /// ItemsTotal uses the same rounded unit price as the checkout.
        /// </summary>
        public DataTable GetPharmacyGroups(int customerId, decimal deliveryCharge)
        {
            string sql = @"
SELECT  ph.PharmacyId,
        ph.PharmacyName,
        COUNT(*)                                                                     AS Lines,
        SUM(ct.Quantity)                                                             AS Units,
        CAST(SUM(ct.Quantity * m.UnitPrice) AS DECIMAL(12,2))                        AS BeforeDiscount,
        CAST(SUM(ct.Quantity * " + UnitPriceExpression + @") AS DECIMAL(12,2))       AS ItemsTotal,
        @DeliveryCharge                                                              AS DeliveryCharge,
        ph.CommissionRate
FROM    Cart ct
        INNER JOIN Medicines  m  ON m.MedicineId  = ct.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId" + BestOfferApply + @"
WHERE   ct.CustomerId = @CustomerId
GROUP BY ph.PharmacyId, ph.PharmacyName, ph.CommissionRate
ORDER BY ph.PharmacyName;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@CustomerId", customerId),
                DbHelper.P("@DeliveryCharge", deliveryCharge));
        }

        /// <summary>True when the basket contains a medicine whose RequiresRx flag is set.</summary>
        public bool ContainsPrescriptionItem(int customerId, int pharmacyId)
        {
            return _db.ExecuteScalarInt(@"
SELECT  COUNT(*)
FROM    Cart ct
        INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId
WHERE   ct.CustomerId = @Cust
  AND   m.RequiresRx  = 1
  AND   (@PharmacyId = 0 OR m.PharmacyId = @PharmacyId);",
                DbHelper.P("@Cust", customerId),
                DbHelper.P("@PharmacyId", pharmacyId)) > 0;
        }

        /// <summary>
        /// Sum of every line in the basket, discounts applied, before delivery.
        /// Screens that also show a discount should add up GetPharmacyGroups
        /// instead, so both figures come from one query.
        /// </summary>
        public decimal GetItemsTotal(int customerId)
        {
            return _db.ExecuteScalarDecimal(@"
SELECT  ISNULL(CAST(SUM(ct.Quantity * " + UnitPriceExpression + @") AS DECIMAL(12,2)), 0)
FROM    Cart ct
        INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId" + BestOfferApply + @"
WHERE   ct.CustomerId = @Cust;",
                DbHelper.P("@Cust", customerId));
        }

        /// <summary>
        /// How much the discounts are saving the customer right now: the list
        /// total minus the rounded discounted total, so it always equals
        /// "before discount" minus GetItemsTotal.
        /// </summary>
        public decimal GetDiscountTotal(int customerId)
        {
            return _db.ExecuteScalarDecimal(@"
SELECT  ISNULL(CAST(SUM(ct.Quantity * m.UnitPrice) - SUM(ct.Quantity * " + UnitPriceExpression + @") AS DECIMAL(12,2)), 0)
FROM    Cart ct
        INNER JOIN Medicines m ON m.MedicineId = ct.MedicineId" + BestOfferApply + @"
WHERE   ct.CustomerId = @Cust;",
                DbHelper.P("@Cust", customerId));
        }
    }
}

using System.Data;
using PharmaLinkApp.Database;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Time limited percentage discounts (requirements 14 and 29).
    ///
    /// The discounted price is always calculated inside the SQL query, never in
    /// C#, so the offers screen, the medicine details screen, the cart and the
    /// invoice all print the same number with no chance of the four disagreeing.
    /// </summary>
    public class OfferService
    {
        private readonly DbHelper _db = new DbHelper();

        // ---------------------------------------------------------------------
        //  CUSTOMER
        // ---------------------------------------------------------------------

        /// <summary>
        /// Requirement 29. Only offers where today falls between StartDate and
        /// EndDate, on a medicine a customer can actually buy right now: listed
        /// by its owner, not expired, in stock, at a pharmacy that is Approved.
        /// Expired offers are filtered out by the query rather than by the form,
        /// so nothing stale can ever be displayed.
        ///
        /// A medicine can have two running offers at once. The cart and the
        /// details screen apply only the highest percentage, so this list shows
        /// one row per medicine, its best offer (ties go to the older offer),
        /// and the discount on screen is always the discount the cart charges.
        /// </summary>
        public DataTable GetActiveOffers(int categoryId, string area)
        {
            const string sql = @"
SELECT  OfferId, OfferTitle, MedicineName, Strength, CategoryName,
        PharmacyName, Area, OriginalPrice, DiscountPercent, DiscountedPrice,
        YouSave, EndDate, MedicineId, Stock
FROM   (SELECT  o.OfferId, o.OfferTitle, m.MedicineName, m.Strength, c.CategoryName,
                ph.PharmacyName, ph.Area,
                m.UnitPrice                                                          AS OriginalPrice,
                o.DiscountPercent,
                CAST(m.UnitPrice * (1 - o.DiscountPercent / 100.0) AS DECIMAL(10,2)) AS DiscountedPrice,
                CAST(m.UnitPrice * (o.DiscountPercent / 100.0) AS DECIMAL(10,2))     AS YouSave,
                o.EndDate,
                m.MedicineId,
                m.Stock,
                ROW_NUMBER() OVER (PARTITION BY o.MedicineId
                                   ORDER BY o.DiscountPercent DESC, o.OfferId ASC)  AS OfferRank
        FROM    Offers o
                INNER JOIN Medicines  m  ON m.MedicineId  = o.MedicineId
                INNER JOIN Categories c  ON c.CategoryId  = m.CategoryId
                INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
        WHERE   o.IsActive   = 1
          AND   m.IsActive   = 1
          AND   CAST(GETDATE() AS DATE) BETWEEN o.StartDate AND o.EndDate
          AND   m.ExpiryDate > CAST(GETDATE() AS DATE)
          AND   m.Stock      > 0
          AND   ph.Status    = 'Approved'
          AND   (@CategoryId = 0  OR m.CategoryId = @CategoryId)
          AND   (@Area       = '' OR ph.Area      = @Area)) best
WHERE   OfferRank = 1
ORDER BY DiscountPercent DESC, MedicineName;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@CategoryId", categoryId),
                DbHelper.P("@Area", area ?? ""));
        }

        /// <summary>How many rows GetActiveOffers shows with no filter: discounted medicines a customer can buy.</summary>
        public int CountActiveOffers()
        {
            return _db.ExecuteScalarInt(@"
SELECT  COUNT(DISTINCT o.MedicineId)
FROM    Offers o
        INNER JOIN Medicines  m  ON m.MedicineId  = o.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   o.IsActive = 1 AND m.IsActive = 1 AND ph.Status = 'Approved'
  AND   m.Stock > 0 AND m.ExpiryDate > CAST(GETDATE() AS DATE)
  AND   CAST(GETDATE() AS DATE) BETWEEN o.StartDate AND o.EndDate;");
        }

        // ---------------------------------------------------------------------
        //  PHARMACY OWNER  (requirement 14)
        // ---------------------------------------------------------------------

        /// <summary>
        /// The owner's own offers only; the join to Medicines carries the PharmacyId filter.
        /// MedicineId is returned so selecting an offer can select its medicine
        /// in the editor, and the price preview uses that medicine's price.
        /// </summary>
        public DataTable GetForPharmacy(int pharmacyId)
        {
            const string sql = @"
SELECT  o.OfferId, o.MedicineId, o.OfferTitle, m.MedicineName, m.Strength,
        m.UnitPrice                                                          AS OriginalPrice,
        o.DiscountPercent,
        CAST(m.UnitPrice * (1 - o.DiscountPercent / 100.0) AS DECIMAL(10,2)) AS DiscountedPrice,
        o.StartDate, o.EndDate, o.IsActive,
        CASE WHEN o.IsActive = 0 THEN 'Paused'
             WHEN CAST(GETDATE() AS DATE) <  o.StartDate THEN 'Scheduled'
             WHEN CAST(GETDATE() AS DATE) >  o.EndDate   THEN 'Expired'
             ELSE 'Running' END                                              AS OfferState
FROM    Offers o
        INNER JOIN Medicines m ON m.MedicineId = o.MedicineId
WHERE   m.PharmacyId = @PharmacyId
ORDER BY o.StartDate DESC;";

            return _db.ExecuteTable(sql, DbHelper.P("@PharmacyId", pharmacyId));
        }

        /// <summary>
        /// Creates an offer. The INSERT ... SELECT with the PharmacyId in its
        /// WHERE clause is what stops an owner creating a discount on somebody
        /// else's medicine: if the medicine is not his, no row is inserted.
        /// </summary>
        public bool Create(int medicineId, int pharmacyId, string title, decimal discountPercent,
                           DateTime startDate, DateTime endDate)
        {
            const string sql = @"
INSERT INTO Offers (MedicineId, OfferTitle, DiscountPercent, StartDate, EndDate)
SELECT  m.MedicineId, @Title, @Percent, @StartDate, @EndDate
FROM    Medicines m
WHERE   m.MedicineId = @MedicineId AND m.PharmacyId = @PharmacyId;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@Title", title.Trim()),
                DbHelper.P("@Percent", discountPercent),
                DbHelper.P("@StartDate", startDate.Date),
                DbHelper.P("@EndDate", endDate.Date),
                DbHelper.P("@MedicineId", medicineId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        /// <summary>
        /// Saves an edited offer, including moving it to another medicine. Both
        /// the offer's current medicine and the new one must belong to this
        /// pharmacy, otherwise nothing changes and false is returned.
        /// </summary>
        public bool Update(int offerId, int pharmacyId, int medicineId, string title, decimal discountPercent,
                           DateTime startDate, DateTime endDate)
        {
            const string sql = @"
UPDATE  o
SET     o.MedicineId = @MedicineId, o.OfferTitle = @Title, o.DiscountPercent = @Percent,
        o.StartDate = @StartDate, o.EndDate = @EndDate
FROM    Offers o INNER JOIN Medicines m ON m.MedicineId = o.MedicineId
WHERE   o.OfferId = @OfferId AND m.PharmacyId = @PharmacyId
  AND   EXISTS (SELECT 1 FROM Medicines m2
                WHERE m2.MedicineId = @MedicineId AND m2.PharmacyId = @PharmacyId);";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@MedicineId", medicineId),
                DbHelper.P("@Title", title.Trim()),
                DbHelper.P("@Percent", discountPercent),
                DbHelper.P("@StartDate", startDate.Date),
                DbHelper.P("@EndDate", endDate.Date),
                DbHelper.P("@OfferId", offerId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        /// <summary>Pausing an offer keeps the row, so it can be switched back on later.</summary>
        public bool SetActive(int offerId, int pharmacyId, bool active)
        {
            const string sql = @"
UPDATE  o SET o.IsActive = @Active
FROM    Offers o INNER JOIN Medicines m ON m.MedicineId = o.MedicineId
WHERE   o.OfferId = @OfferId AND m.PharmacyId = @PharmacyId;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@Active", active ? 1 : 0),
                DbHelper.P("@OfferId", offerId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        public bool Delete(int offerId, int pharmacyId)
        {
            const string sql = @"
DELETE  o
FROM    Offers o INNER JOIN Medicines m ON m.MedicineId = o.MedicineId
WHERE   o.OfferId = @OfferId AND m.PharmacyId = @PharmacyId;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@OfferId", offerId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        /// <summary>
        /// Other unpaused offers on the same medicine whose dates overlap the
        /// given range. The form uses it to warn, not to block: two offers may
        /// run together, but only the higher percentage is ever charged, and the
        /// owner should know that before saving. highestPercent is the largest
        /// of them (0 when there are none).
        /// </summary>
        public int CountOverlapping(int pharmacyId, int medicineId, DateTime startDate, DateTime endDate,
                                    int ignoreOfferId, out decimal highestPercent)
        {
            DataTable table = _db.ExecuteTable(@"
SELECT  COUNT(*) AS Overlapping, ISNULL(MAX(o.DiscountPercent), 0) AS Highest
FROM    Offers o INNER JOIN Medicines m ON m.MedicineId = o.MedicineId
WHERE   m.PharmacyId = @PharmacyId
  AND   o.MedicineId = @MedicineId
  AND   o.OfferId   <> @Ignore
  AND   o.IsActive   = 1
  AND   o.StartDate <= @EndDate
  AND   o.EndDate   >= @StartDate;",
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@MedicineId", medicineId),
                DbHelper.P("@Ignore", ignoreOfferId),
                DbHelper.P("@StartDate", startDate.Date),
                DbHelper.P("@EndDate", endDate.Date));

            highestPercent = table.Rows.Count == 0 ? 0m : DbHelper.GetDecimal(table.Rows[0], "Highest");
            return table.Rows.Count == 0 ? 0 : DbHelper.GetInt(table.Rows[0], "Overlapping");
        }

        public int CountRunningForPharmacy(int pharmacyId)
        {
            return _db.ExecuteScalarInt(@"
SELECT  COUNT(*)
FROM    Offers o INNER JOIN Medicines m ON m.MedicineId = o.MedicineId
WHERE   m.PharmacyId = @Id AND o.IsActive = 1
  AND   CAST(GETDATE() AS DATE) BETWEEN o.StartDate AND o.EndDate;",
                DbHelper.P("@Id", pharmacyId));
        }
    }
}

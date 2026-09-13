using System.Data;
using PharmaLinkApp.Database;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Ratings and comments (requirements 15, 21, 24 and 8).
    ///
    /// A review points back at the order it came from, so only a customer who
    /// actually received the medicine can rate it, and the UNIQUE constraint on
    /// (CustomerId, MedicineId, OrderId) stops the same purchase being rated
    /// twice. Moderation hides a review instead of deleting it.
    /// </summary>
    public class ReviewService
    {
        private readonly DbHelper _db = new DbHelper();

        // ---------------------------------------------------------------------
        //  CUSTOMER
        // ---------------------------------------------------------------------

        /// <summary>
        /// Writes a review, but only when the WHERE EXISTS clause can prove the
        /// customer bought that medicine on that order and the order has been
        /// delivered. The form disables the button too, but this is the rule
        /// that actually holds.
        /// </summary>
        public bool AddReview(int customerId, int medicineId, int orderId, int rating, string comment, out string message)
        {
            const string sql = @"
INSERT INTO Reviews (CustomerId, MedicineId, OrderId, Rating, Comment)
SELECT  @CustomerId, @MedicineId, @OrderId, @Rating, @Comment
WHERE   EXISTS (SELECT 1
                FROM   OrderItems oi
                       INNER JOIN Orders o ON o.OrderId = oi.OrderId
                WHERE  oi.OrderId    = @OrderId
                  AND  oi.MedicineId = @MedicineId
                  AND  o.CustomerId  = @CustomerId
                  AND  o.Status      = 'Delivered');";

            try
            {
                int rows = _db.ExecuteNonQuery(sql,
                    DbHelper.P("@CustomerId", customerId),
                    DbHelper.P("@MedicineId", medicineId),
                    DbHelper.P("@OrderId", orderId),
                    DbHelper.P("@Rating", rating),
                    DbHelper.P("@Comment", comment));

                if (rows == 1)
                {
                    message = "Thank you, your review has been posted.";
                    return true;
                }

                message = "You can only review a medicine from an order that has been delivered to you.";
                return false;
            }
            catch (Exception ex)
            {
                // The UNIQUE constraint fires when the same purchase is rated twice.
                // DbHelper wraps the SqlException, so the constraint name is on the
                // inner exception, not on the friendly outer message.
                string raw = (ex.InnerException != null ? ex.InnerException.Message : "") + ex.Message;
                if (raw.Contains("UQ_Reviews_OneEach"))
                    message = "You have already reviewed this medicine on this order.";
                else
                    message = DbHelper.Describe(ex);
                return false;
            }
        }

        /// <summary>The medicines on one delivered order that have not been reviewed yet.</summary>
        public DataTable GetReviewableItems(int orderId, int customerId)
        {
            const string sql = @"
SELECT  m.MedicineId, m.MedicineName, m.Strength, oi.Quantity, oi.UnitPrice
FROM    OrderItems oi
        INNER JOIN Orders    o ON o.OrderId    = oi.OrderId
        INNER JOIN Medicines m ON m.MedicineId = oi.MedicineId
WHERE   oi.OrderId   = @OrderId
  AND   o.CustomerId = @CustomerId
  AND   o.Status     = 'Delivered'
  AND   NOT EXISTS (SELECT 1 FROM Reviews r
                    WHERE r.OrderId    = oi.OrderId
                      AND r.MedicineId = oi.MedicineId
                      AND r.CustomerId = @CustomerId)
ORDER BY m.MedicineName;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@OrderId", orderId),
                DbHelper.P("@CustomerId", customerId));
        }

        /// <summary>
        /// Requirement 23. Reviews store only a CustomerId, so the join to Users
        /// is what turns a number into the reviewer's name on screen. Hidden
        /// reviews are excluded here rather than deleted at source.
        /// </summary>
        public DataTable GetForMedicine(int medicineId)
        {
            const string sql = @"
SELECT  r.ReviewId, u.FullName AS ReviewerName, r.Rating, r.Comment, r.ReviewDate
FROM    Reviews r
        INNER JOIN Users     u ON u.UserId     = r.CustomerId
        INNER JOIN Medicines m ON m.MedicineId = r.MedicineId
WHERE   r.MedicineId = @MedicineId
  AND   r.IsHidden   = 0
ORDER BY r.ReviewDate DESC;";

            return _db.ExecuteTable(sql, DbHelper.P("@MedicineId", medicineId));
        }

        public decimal GetAverageForMedicine(int medicineId)
        {
            return _db.ExecuteScalarDecimal(@"
SELECT ISNULL(CAST(AVG(CAST(Rating AS DECIMAL(4,2))) AS DECIMAL(4,2)), 0)
FROM   Reviews WHERE MedicineId = @Id AND IsHidden = 0;",
                DbHelper.P("@Id", medicineId));
        }

        // ---------------------------------------------------------------------
        //  PHARMACY OWNER  (requirement 15 - read only by design, plus Report)
        // ---------------------------------------------------------------------

        public DataTable GetForPharmacy(int pharmacyId, int minRating, int maxRating)
        {
            const string sql = @"
SELECT  r.ReviewId, u.FullName AS ReviewerName, m.MedicineName, m.Strength,
        r.Rating, r.Comment, r.ReviewDate, r.OrderId, r.IsReported
FROM    Reviews r
        INNER JOIN Users     u ON u.UserId     = r.CustomerId
        INNER JOIN Medicines m ON m.MedicineId = r.MedicineId
WHERE   m.PharmacyId = @PharmacyId
  AND   r.IsHidden   = 0
  AND   r.Rating BETWEEN @MinRating AND @MaxRating
ORDER BY r.ReviewDate DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@MinRating", minRating),
                DbHelper.P("@MaxRating", maxRating));
        }

        /// <summary>
        /// The owner flags a review for the Super Admin. The owner cannot hide
        /// or change the review; this only sets IsReported with a reason and the
        /// time. The join to Medicines scopes it to reviews of this pharmacy's
        /// own medicines, and IsReported = 0 stops the same review being reported
        /// twice while the first report is still open. Returns false when
        /// nothing changed.
        /// </summary>
        public bool Report(int reviewId, int pharmacyId, string reason)
        {
            const string sql = @"
UPDATE  r
SET     r.IsReported = 1, r.ReportReason = @Reason, r.ReportedAt = SYSDATETIME()
FROM    Reviews r INNER JOIN Medicines m ON m.MedicineId = r.MedicineId
WHERE   r.ReviewId   = @Id
  AND   m.PharmacyId = @PharmacyId
  AND   r.IsReported = 0
  AND   r.IsHidden   = 0;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@Reason", (reason ?? "").Trim()),
                DbHelper.P("@Id", reviewId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        public decimal GetAverageForPharmacy(int pharmacyId)
        {
            return _db.ExecuteScalarDecimal(@"
SELECT  ISNULL(CAST(AVG(CAST(r.Rating AS DECIMAL(4,2))) AS DECIMAL(4,2)), 0)
FROM    Reviews r
        INNER JOIN Medicines m ON m.MedicineId = r.MedicineId
WHERE   m.PharmacyId = @Id AND r.IsHidden = 0;",
                DbHelper.P("@Id", pharmacyId));
        }

        public int CountForPharmacy(int pharmacyId)
        {
            return _db.ExecuteScalarInt(@"
SELECT  COUNT(*)
FROM    Reviews r INNER JOIN Medicines m ON m.MedicineId = r.MedicineId
WHERE   m.PharmacyId = @Id AND r.IsHidden = 0;",
                DbHelper.P("@Id", pharmacyId));
        }

        // ---------------------------------------------------------------------
        //  SUPER ADMIN MODERATION  (requirement 8)
        // ---------------------------------------------------------------------

        /// <summary>
        /// The moderation queue. Every row carries the order number that proves
        /// the purchase, so a review always traces back to a real delivery.
        /// With reportedOnly the queue shows only reviews a pharmacy owner has
        /// reported, whatever their rating, with the owner's reason.
        ///
        /// PharmacyId rides along (the screen hides it) so the Super Admin can
        /// warn the shop behind a review, or open its record, without a second
        /// lookup by name - two shops could one day share a name, an id cannot.
        /// </summary>
        public DataTable GetModerationQueue(int maxRating, bool includeHidden, bool reportedOnly)
        {
            const string sql = @"
SELECT  r.ReviewId, u.FullName AS Reviewer, m.MedicineName, ph.PharmacyName,
        r.Rating, r.Comment, r.ReviewDate, r.OrderId, r.IsHidden,
        r.IsReported, r.ReportReason, r.ReportedAt, m.PharmacyId
FROM    Reviews r
        INNER JOIN Users      u  ON u.UserId     = r.CustomerId
        INNER JOIN Medicines  m  ON m.MedicineId = r.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   (@ReportedOnly = 1 OR r.Rating <= @MaxRating)
  AND   (@ReportedOnly = 0 OR r.IsReported = 1)
  AND   (@IncludeHidden = 1 OR r.IsHidden = 0)
ORDER BY r.IsReported DESC, r.ReviewDate DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@MaxRating", maxRating),
                DbHelper.P("@IncludeHidden", includeHidden ? 1 : 0),
                DbHelper.P("@ReportedOnly", reportedOnly ? 1 : 0));
        }

        /// <summary>
        /// Hiding sets IsHidden to 1 rather than deleting the row, so the review
        /// disappears from the customer screens and from every average rating
        /// calculation, and the decision can be reversed with Restore.
        /// Hiding also closes any open report on the review (IsReported = 0),
        /// because hiding is the Super Admin's answer to it; the reason and
        /// time are kept for the record.
        /// </summary>
        public bool SetHidden(int reviewId, bool hidden)
        {
            return _db.ExecuteNonQuery(@"
UPDATE Reviews
SET    IsHidden   = @Hidden,
       IsReported = CASE WHEN @Hidden = 1 THEN 0 ELSE IsReported END
WHERE  ReviewId = @Id;",
                DbHelper.P("@Hidden", hidden ? 1 : 0),
                DbHelper.P("@Id", reviewId)) == 1;
        }

        /// <summary>
        /// The Super Admin looked at a reported review and decided it stays
        /// visible. Clears the report without hiding anything. Returns false
        /// when the review was not reported (someone else already dealt with it).
        /// </summary>
        public bool DismissReport(int reviewId)
        {
            return _db.ExecuteNonQuery(
                "UPDATE Reviews SET IsReported = 0 WHERE ReviewId = @Id AND IsReported = 1;",
                DbHelper.P("@Id", reviewId)) == 1;
        }
    }
}

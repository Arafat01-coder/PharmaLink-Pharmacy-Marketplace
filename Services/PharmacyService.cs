using System.Data;
using Microsoft.Data.SqlClient;
using PharmaLinkApp.Database;
using PharmaLinkApp.Models;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// The Super Admin's control over shops (approve, reject, suspend,
    /// commission rate) and the pharmacy owner's control over his own shop
    /// profile.
    ///
    /// Pharmacy status life cycle:
    ///     Pending  --Approve-->  Approved  --Suspend-->  Suspended
    ///     Pending  --Reject--->  Rejected  --Approve-->  Approved
    ///     Suspended --Reinstate--> Approved
    /// Every status change checks the status it starts from inside its UPDATE,
    /// so a stale screen (or a second Super Admin) cannot, say, suspend a shop
    /// that is still Pending. The methods return how many pharmacies changed,
    /// 0 meaning "nothing happened, the shop was not in the expected state".
    ///
    /// Medicines.IsActive is never touched here. It means only "the owner has
    /// listed this medicine"; whether customers can see it is decided by the
    /// pharmacy's Status = 'Approved' in the customer-facing queries.
    /// </summary>
    public class PharmacyService
    {
        private readonly DbHelper _db = new DbHelper();

        // ---------------------------------------------------------------------
        //  SUPER ADMIN
        // ---------------------------------------------------------------------

        /// <summary>
        /// Requirement 2 and 3: every pharmacy with its owner, licence, area and
        /// status, plus its order count so the screen knows whether Delete is
        /// possible.
        ///
        /// WarningState is the current warning boiled down to a short cell,
        /// "Warned 13 Sep 26" while the owner has not read it and
        /// "Read 14 Sep 26" once they have, or empty for a shop never warned.
        /// It is built here rather than in the form so the grid can stay a
        /// plain binding; the culture is pinned so the month is always English.
        /// WarningMessage comes along for the cell's tooltip.
        /// </summary>
        public DataTable Search(string keyword, string status, string area)
        {
            const string sql = @"
SELECT  p.PharmacyId, p.PharmacyName, u.FullName AS OwnerName, u.Email AS OwnerEmail,
        p.LicenseNo, p.Area, p.ContactPhone, p.CommissionRate, p.Status, p.RegisteredAt,
        (SELECT COUNT(*) FROM Medicines m WHERE m.PharmacyId = p.PharmacyId) AS Medicines,
        (SELECT COUNT(*) FROM Orders    o WHERE o.PharmacyId = p.PharmacyId) AS Orders,
        ISNULL((SELECT CAST(AVG(CAST(r.Rating AS DECIMAL(4,2))) AS DECIMAL(4,2))
                FROM   Reviews r
                       INNER JOIN Medicines m2 ON m2.MedicineId = r.MedicineId
                WHERE  m2.PharmacyId = p.PharmacyId AND r.IsHidden = 0), 0) AS AverageRating,
        CASE WHEN p.WarnedAt IS NULL              THEN ''
             WHEN p.WarningAcknowledgedAt IS NULL THEN 'Warned ' + FORMAT(p.WarnedAt, 'dd MMM yy', 'en-US')
             ELSE 'Read ' + FORMAT(p.WarningAcknowledgedAt, 'dd MMM yy', 'en-US')
        END AS WarningState,
        p.WarningMessage
FROM    Pharmacies p
        INNER JOIN Users u ON u.UserId = p.OwnerId
WHERE   (@Keyword = '' OR p.PharmacyName LIKE '%' + @Keyword + '%'
                       OR p.LicenseNo    LIKE '%' + @Keyword + '%'
                       OR u.FullName     LIKE '%' + @Keyword + '%')
  AND   (@Status  = '' OR p.Status = @Status)
  AND   (@Area    = '' OR p.Area   = @Area)
ORDER BY CASE p.Status WHEN 'Pending' THEN 0 WHEN 'Approved' THEN 1 WHEN 'Suspended' THEN 2 ELSE 3 END,
         p.PharmacyName;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@Keyword", keyword ?? ""),
                DbHelper.P("@Status", status ?? ""),
                DbHelper.P("@Area", area ?? ""));
        }

        public DataTable GetPending()
        {
            const string sql = @"
SELECT  p.PharmacyId, p.PharmacyName, u.FullName AS OwnerName, p.LicenseNo,
        p.Area, p.ContactPhone, p.RegisteredAt
FROM    Pharmacies p
        INNER JOIN Users u ON u.UserId = p.OwnerId
WHERE   p.Status = 'Pending'
ORDER BY p.RegisteredAt;";

            return _db.ExecuteTable(sql);
        }

        /// <summary>
        /// Approval flips two rows, because the pharmacy record and the login
        /// account are separate concerns: the shop becomes Approved and the
        /// owner's account becomes Active so he can finally log in. Allowed from
        /// Pending, and from Rejected so a rejection can be reversed.
        /// Returns 1 when the shop was approved, 0 when it was not in a state
        /// that can be approved.
        /// </summary>
        public int Approve(int pharmacyId)
        {
            return ChangeStatus(pharmacyId, "Pending", "Rejected", "Approved", "Active");
        }

        /// <summary>
        /// Suspension flips the pharmacy and its owner's login account, inside
        /// one transaction. Nothing is deleted anywhere and the medicines are
        /// left alone, so past orders, the invoices customers already hold and
        /// the owner's own listing all stay exactly as they were; customers stop
        /// seeing the shop because it is no longer Approved. Only an Approved
        /// shop can be suspended.
        /// </summary>
        public int Suspend(int pharmacyId)
        {
            return ChangeStatus(pharmacyId, "Approved", null, "Suspended", "Suspended");
        }

        /// <summary>
        /// Puts a suspended shop back on the platform: Approved again, owner
        /// Active again. Only a Suspended shop can be reinstated.
        /// </summary>
        public int Reinstate(int pharmacyId)
        {
            return ChangeStatus(pharmacyId, "Suspended", null, "Approved", "Active");
        }

        /// <summary>
        /// Rejects a Pending registration. The rows are kept rather than erased,
        /// so the licence number stays taken and the decision can be reversed
        /// with Approve: the pharmacy becomes Rejected and the owner's account
        /// Suspended.
        /// </summary>
        public int Reject(int pharmacyId)
        {
            return ChangeStatus(pharmacyId, "Pending", null, "Rejected", "Suspended");
        }

        /// <summary>
        /// The one implementation of every pharmacy status change. The guard on
        /// the starting status lives in the UPDATE itself, so a check and the
        /// change can never be separated by someone else's change. XACT_ABORT
        /// plus TRY/CATCH means any error rolls back both rows and is re-thrown
        /// to DbHelper, which turns it into readable English.
        /// </summary>
        private int ChangeStatus(int pharmacyId, string fromStatus, string otherFromStatus,
                                 string toStatus, string ownerStatus)
        {
            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Rows INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Lock the owner's Users row BEFORE the Pharmacies row. AuthService.SetUserStatus
    -- locks Users first and Pharmacies second, and taking the two in the same
    -- order everywhere is what stops a Super Admin suspending the owner and
    -- another reinstating the shop at the same moment from deadlocking.
    DECLARE @OwnerId INT = (SELECT OwnerId FROM Pharmacies WHERE PharmacyId = @PharmacyId);

    SELECT  @OwnerId = u.UserId
    FROM    Users u WITH (UPDLOCK, HOLDLOCK)
    WHERE   u.UserId = @OwnerId;

    UPDATE  Pharmacies
    SET     Status = @ToStatus
    WHERE   PharmacyId = @PharmacyId
      AND   (Status = @FromStatus OR Status = @OtherFromStatus);
    SET @Rows = @@ROWCOUNT;

    IF @Rows = 1
        UPDATE  Users
        SET     Status = @OwnerStatus
        WHERE   UserId = @OwnerId;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT @Rows;";

            return _db.ExecuteScalarInt(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@FromStatus", fromStatus),
                DbHelper.P("@OtherFromStatus", otherFromStatus ?? fromStatus),
                DbHelper.P("@ToStatus", toStatus),
                DbHelper.P("@OwnerStatus", ownerStatus));
        }

        /// <summary>
        /// Requirement 3: delete a shop entirely. Only possible while it has no
        /// order history; a shop that has traded is suspended instead, because
        /// deleting it would destroy invoices customers already hold.
        /// </summary>
        public bool Delete(int pharmacyId, out string message)
        {
            int orders = _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Orders WHERE PharmacyId = @Id;",
                DbHelper.P("@Id", pharmacyId));

            if (orders > 0)
            {
                message = "This pharmacy has " + orders + " order(s) in its history, so it cannot be deleted. " +
                          "Suspend it instead - suspension hides it from customers without destroying past invoices.";
                return false;
            }

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            // The owner's UserId is read first, because the Pharmacies row
                            // is the only thing that points at it and that row is about to go.
                            int ownerId;
                            using (SqlCommand read = new SqlCommand(
                                "SELECT OwnerId FROM Pharmacies WHERE PharmacyId = @Id;", conn, tx))
                            {
                                read.Parameters.AddWithValue("@Id", pharmacyId);
                                object value = read.ExecuteScalar();
                                ownerId = value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
                            }

                            if (ownerId == 0)
                            {
                                tx.Rollback();
                                message = "That pharmacy no longer exists - it may already have been deleted.";
                                return false;
                            }

                            ExecuteInTx(conn, tx,
                                "DELETE FROM Offers WHERE MedicineId IN (SELECT MedicineId FROM Medicines WHERE PharmacyId = @Id);", pharmacyId);
                            ExecuteInTx(conn, tx,
                                "DELETE FROM Cart WHERE MedicineId IN (SELECT MedicineId FROM Medicines WHERE PharmacyId = @Id);", pharmacyId);
                            ExecuteInTx(conn, tx,
                                "DELETE FROM Medicines WHERE PharmacyId = @Id;", pharmacyId);

                            // Pharmacies before Users. FK_Pharmacies_Owner points from
                            // Pharmacies to Users and has no ON DELETE CASCADE, so removing
                            // the owner while the shop row still references it is a foreign
                            // key violation and the whole transaction rolls back.
                            ExecuteInTx(conn, tx,
                                "DELETE FROM Pharmacies WHERE PharmacyId = @Id;", pharmacyId);

                            // The owner may have shopped as a customer with the same
                            // account's cart; clear that too so the FK allows the delete.
                            using (SqlCommand removeOwner = new SqlCommand(
                                "DELETE FROM Cart WHERE CustomerId = @OwnerId; DELETE FROM Users WHERE UserId = @OwnerId;", conn, tx))
                            {
                                removeOwner.Parameters.AddWithValue("@OwnerId", ownerId);
                                removeOwner.ExecuteNonQuery();
                            }

                            tx.Commit();
                            message = "Pharmacy deleted.";
                            return true;
                        }
                        catch (Exception ex)
                        {
                            try { tx.Rollback(); } catch (InvalidOperationException) { /* already rolled back by the server */ }
                            message = DbHelper.Describe(ex);
                            return false;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // conn.Open() failed: the server could not be reached.
                message = DbHelper.Describe(ex);
                return false;
            }
        }

        private static void ExecuteInTx(SqlConnection conn, SqlTransaction tx, string sql, int pharmacyId)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", pharmacyId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Requirement 9. The rate is a column on Pharmacies rather than a
        /// constant in the code, and the value is checked here and again by
        /// CK_Pharmacies_Comm. Changing it affects only orders placed from this
        /// moment on, because every order froze its own commission at checkout.
        /// </summary>
        public bool SetCommissionRate(int pharmacyId, decimal rate)
        {
            if (rate < 0m || rate > 30m) return false;
            return _db.ExecuteNonQuery(
                "UPDATE Pharmacies SET CommissionRate = @Rate WHERE PharmacyId = @Id;",
                DbHelper.P("@Rate", rate),
                DbHelper.P("@Id", pharmacyId)) == 1;
        }

        // ---------------------------------------------------------------------
        //  WARNINGS  (Super Admin -> pharmacy owner)
        //  A pharmacy holds at most one current warning in three columns on
        //  Pharmacies. A warning is a notice, not a sanction: it changes no
        //  status and hides nothing, so it can be sent to a shop in any state.
        // ---------------------------------------------------------------------

        /// <summary>Shortest and longest warning text accepted, matching NVARCHAR(500) on Pharmacies.WarningMessage.</summary>
        public const int WarningMinLength = 10;
        public const int WarningMaxLength = 500;

        /// <summary>
        /// Sends (or replaces) the pharmacy's warning. Overwriting rather than
        /// keeping a history is deliberate: the owner's banner shows exactly one
        /// notice, and clearing WarningAcknowledgedAt makes a repeated warning
        /// show again even if the previous one was already read.
        /// Returns the number of pharmacies changed: 1, or 0 when the text is
        /// outside the allowed length or the pharmacy no longer exists.
        /// </summary>
        public int WarnPharmacy(int pharmacyId, string message)
        {
            string text = (message ?? "").Trim();
            if (text.Length < WarningMinLength || text.Length > WarningMaxLength) return 0;

            const string sql = @"
UPDATE  Pharmacies
SET     WarningMessage        = @Message,
        WarnedAt              = SYSDATETIME(),
        WarningAcknowledgedAt = NULL
WHERE   PharmacyId = @Id;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@Message", text),
                DbHelper.P("@Id", pharmacyId));
        }

        /// <summary>
        /// The pharmacy's current warning, for the owner's dashboard banner.
        /// Returns null when the pharmacy does not exist; otherwise a Pharmacy
        /// carrying only its id, name and the three warning fields (WarnedAt is
        /// null when the shop has never been warned). HasUnreadWarning on the
        /// result says whether the banner should show.
        /// </summary>
        public Pharmacy GetWarning(int pharmacyId)
        {
            const string sql = @"
SELECT  PharmacyId, PharmacyName, WarningMessage, WarnedAt, WarningAcknowledgedAt
FROM    Pharmacies
WHERE   PharmacyId = @Id;";

            DataTable table = _db.ExecuteTable(sql, DbHelper.P("@Id", pharmacyId));
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            return new Pharmacy
            {
                PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                PharmacyName = DbHelper.GetString(row, "PharmacyName"),
                WarningMessage = DbHelper.GetString(row, "WarningMessage"),
                WarnedAt = row["WarnedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["WarnedAt"]),
                WarningAcknowledgedAt = row["WarningAcknowledgedAt"] == DBNull.Value
                    ? (DateTime?)null : Convert.ToDateTime(row["WarningAcknowledgedAt"])
            };
        }

        /// <summary>
        /// The owner's "I've read this". The WHERE carries the owner's own
        /// PharmacyId (requirement 18) and WarningAcknowledgedAt IS NULL, so the
        /// first acknowledgement time is kept - the Super Admin sees when the
        /// owner really read it, not when they last clicked. Returns false when
        /// nothing changed (already acknowledged, or no such pharmacy).
        /// </summary>
        public bool AcknowledgeWarning(int pharmacyId)
        {
            const string sql = @"
UPDATE  Pharmacies
SET     WarningAcknowledgedAt = SYSDATETIME()
WHERE   PharmacyId = @Id
  AND   WarnedAt IS NOT NULL
  AND   WarningAcknowledgedAt IS NULL;";

            return _db.ExecuteNonQuery(sql, DbHelper.P("@Id", pharmacyId)) == 1;
        }

        // ---------------------------------------------------------------------
        //  PHARMACY OWNER
        // ---------------------------------------------------------------------

        public Pharmacy GetById(int pharmacyId)
        {
            const string sql = @"
SELECT  p.PharmacyId, p.OwnerId, p.PharmacyName, p.LicenseNo, p.Area, p.Address,
        p.ContactPhone, p.LogoPath, p.CommissionRate, p.Status, p.RegisteredAt,
        u.FullName AS OwnerName
FROM    Pharmacies p
        INNER JOIN Users u ON u.UserId = p.OwnerId
WHERE   p.PharmacyId = @Id;";

            DataTable table = _db.ExecuteTable(sql, DbHelper.P("@Id", pharmacyId));
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            return new Pharmacy
            {
                PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                OwnerId = DbHelper.GetInt(row, "OwnerId"),
                PharmacyName = DbHelper.GetString(row, "PharmacyName"),
                LicenseNo = DbHelper.GetString(row, "LicenseNo"),
                Area = DbHelper.GetString(row, "Area"),
                Address = DbHelper.GetString(row, "Address"),
                ContactPhone = DbHelper.GetString(row, "ContactPhone"),
                LogoPath = DbHelper.GetString(row, "LogoPath"),
                CommissionRate = DbHelper.GetDecimal(row, "CommissionRate"),
                Status = DbHelper.GetString(row, "Status"),
                RegisteredAt = DbHelper.GetDate(row, "RegisteredAt"),
                OwnerName = DbHelper.GetString(row, "OwnerName")
            };
        }

        /// <summary>
        /// Requirement 11. The WHERE clause carries PharmacyId so an owner can
        /// never edit another shop. LicenseNo is deliberately not updatable:
        /// changing it would mean a new licence and a fresh approval.
        /// </summary>
        public bool UpdateProfile(int pharmacyId, string name, string area, string address, string contactPhone, string logoPath)
        {
            const string sql = @"
UPDATE  Pharmacies
SET     PharmacyName = @Name, Area = @Area, Address = @Address,
        ContactPhone = @Phone, LogoPath = @Logo
WHERE   PharmacyId = @Id;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@Name", name.Trim()),
                DbHelper.P("@Area", area.Trim()),
                DbHelper.P("@Address", address.Trim()),
                DbHelper.P("@Phone", contactPhone.Trim()),
                DbHelper.P("@Logo", string.IsNullOrWhiteSpace(logoPath) ? null : logoPath),
                DbHelper.P("@Id", pharmacyId)) == 1;
        }

        // ---------------------------------------------------------------------
        //  SHARED LOOKUPS
        // ---------------------------------------------------------------------

        /// <summary>The distinct areas, used by the customer's Area filter.</summary>
        public List<string> GetAreas(bool approvedOnly)
        {
            List<string> areas = new List<string>();
            DataTable table = _db.ExecuteTable(
                "SELECT DISTINCT Area FROM Pharmacies WHERE (@ApprovedOnly = 0 OR Status = 'Approved') ORDER BY Area;",
                DbHelper.P("@ApprovedOnly", approvedOnly ? 1 : 0));

            foreach (DataRow row in table.Rows)
                areas.Add(DbHelper.GetString(row, "Area"));

            return areas;
        }

        /// <summary>Approved pharmacies only, for the customer's Pharmacy filter.</summary>
        public List<Pharmacy> GetApprovedList()
        {
            List<Pharmacy> list = new List<Pharmacy>();
            DataTable table = _db.ExecuteTable(
                "SELECT PharmacyId, PharmacyName, Area FROM Pharmacies WHERE Status = 'Approved' ORDER BY PharmacyName;");

            foreach (DataRow row in table.Rows)
            {
                list.Add(new Pharmacy
                {
                    PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                    PharmacyName = DbHelper.GetString(row, "PharmacyName"),
                    Area = DbHelper.GetString(row, "Area")
                });
            }
            return list;
        }

        public int CountByStatus(string status)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Pharmacies WHERE Status = @Status;",
                DbHelper.P("@Status", status));
        }
    }
}

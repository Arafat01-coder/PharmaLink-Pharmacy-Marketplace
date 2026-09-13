using System.Configuration;
using System.Data;
using PharmaLinkApp.Database;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Prescription control (requirements 16 and 26).
    ///
    /// A medicine flagged RequiresRx cannot be dispatched until the customer has
    /// uploaded a photograph of the doctor's prescription and the pharmacy owner
    /// has approved that image.
    ///
    /// An order may collect several Prescriptions rows over its life: a
    /// rejected photograph is never overwritten, the customer uploads a new one
    /// instead. The CURRENT prescription of an order is always the row with the
    /// highest PrescriptionId, and every rule below (upload, verify, confirm)
    /// reads that row only, so an old rejection can never block a newer
    /// approval and an old approval can never unlock a newer, unchecked image.
    /// </summary>
    public class PrescriptionService
    {
        private readonly DbHelper _db = new DbHelper();

        private const long MaxImageBytes = 2 * 1024 * 1024;

        /// <summary>The folder named in App.config, as written there (normally relative).</summary>
        private static string ConfiguredFolder
        {
            get
            {
                string configured = ConfigurationManager.AppSettings["PrescriptionFolder"];
                if (string.IsNullOrWhiteSpace(configured))
                    configured = Path.Combine("Uploads", "Prescriptions");
                return configured;
            }
        }

        /// <summary>
        /// The absolute upload folder. Relative settings are resolved against
        /// AppContext.BaseDirectory (the folder the .exe runs from), never the
        /// current working directory, which changes when a file dialog is used.
        /// </summary>
        private static string UploadFolderFullPath =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ConfiguredFolder));

        /// <summary>Where uploaded images are copied to, taken from App.config. Created on first use.</summary>
        public static string UploadFolder
        {
            get
            {
                string full = UploadFolderFullPath;
                if (!Directory.Exists(full)) Directory.CreateDirectory(full);
                return full;
            }
        }

        // =====================================================================
        //  STORING THE IMAGE FILE
        // =====================================================================

        /// <summary>
        /// Validates the chosen image (JPG or PNG, under 2 MB) and copies it into
        /// the upload folder under a new, unique name. The caller writes the
        /// database row afterwards and must call DeleteStoredImage with
        /// <paramref name="storedFullPath"/> if that write fails, so a failed
        /// order never leaves an orphaned photograph behind.
        ///
        /// <paramref name="storedPath"/> is the value to save in
        /// Prescriptions.ImagePath: relative to the application folder when the
        /// upload folder lives inside it, otherwise just the file name. Either
        /// form is understood by ResolveImagePath.
        /// </summary>
        public static bool TryStoreImage(string sourceImagePath, string namePrefix,
                                         out string storedPath, out string storedFullPath, out string message)
        {
            storedPath = "";
            storedFullPath = "";

            try
            {
                FileInfo file = new FileInfo(sourceImagePath ?? "");

                if (!file.Exists)
                {
                    message = "The prescription image could not be found. Please choose it again.";
                    return false;
                }

                string extension = file.Extension.ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    message = "Only JPG and PNG images are accepted.";
                    return false;
                }

                if (file.Length > MaxImageBytes)
                {
                    message = "The image must be smaller than 2 MB.";
                    return false;
                }

                // A GUID fragment keeps two uploads in the same second apart.
                string storedName = namePrefix + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" +
                                    Guid.NewGuid().ToString("N").Substring(0, 8) + extension;

                string folder = UploadFolder;
                storedFullPath = Path.Combine(folder, storedName);
                File.Copy(file.FullName, storedFullPath, false);

                string relative = Path.GetRelativePath(AppContext.BaseDirectory, storedFullPath);
                storedPath = IsSafeRelativePath(relative) ? relative : storedName;

                message = "";
                return true;
            }
            catch (Exception ex)
            {
                DeleteStoredImage(storedFullPath);
                storedPath = "";
                storedFullPath = "";
                message = "The prescription image could not be saved: " + ex.Message;
                return false;
            }
        }

        /// <summary>Best-effort removal of a copied image whose database row was never written.</summary>
        public static void DeleteStoredImage(string storedFullPath)
        {
            if (string.IsNullOrWhiteSpace(storedFullPath)) return;
            try
            {
                if (File.Exists(storedFullPath)) File.Delete(storedFullPath);
            }
            catch
            {
                // Leaving an unreferenced file behind is harmless; failing the
                // caller's error path over it would not be.
            }
        }

        // =====================================================================
        //  CUSTOMER: RE-UPLOAD ON AN EXISTING ORDER
        // =====================================================================

        /// <summary>
        /// Attaches a new prescription to an order that is already placed. This
        /// is the re-upload path from My Orders; the first prescription of an
        /// order is written by OrderService.Checkout inside the checkout
        /// transaction.
        ///
        /// The database refuses the row unless the order belongs to this
        /// customer, is still 'Placed', actually contains a prescription only
        /// medicine, and its current prescription is missing or Rejected. The
        /// order row is locked (UPDLOCK, HOLDLOCK) while that is checked, so two
        /// uploads clicked at once cannot both become Pending.
        /// </summary>
        public bool Upload(int orderId, int customerId, string sourceImagePath, string doctorName, out string message)
        {
            string storedPath, storedFullPath;
            if (!TryStoreImage(sourceImagePath, "rx-" + orderId, out storedPath, out storedFullPath, out message))
                return false;

            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Result INT = 0, @OrderStatus NVARCHAR(15), @Current NVARCHAR(15);

    SELECT  @OrderStatus = o.Status
    FROM    Orders o WITH (UPDLOCK, HOLDLOCK)
    WHERE   o.OrderId = @OrderId AND o.CustomerId = @CustomerId;

    SELECT  TOP 1 @Current = p.VerifyStatus
    FROM    Prescriptions p
    WHERE   p.OrderId = @OrderId
    ORDER BY p.PrescriptionId DESC;

    IF @OrderStatus IS NULL
        SET @Result = -1;                                   -- not this customer's order
    ELSE IF @OrderStatus <> 'Placed'
        SET @Result = -2;                                   -- too late to change
    ELSE IF NOT EXISTS (SELECT 1 FROM OrderItems oi
                        INNER JOIN Medicines m ON m.MedicineId = oi.MedicineId
                        WHERE oi.OrderId = @OrderId AND m.RequiresRx = 1)
        SET @Result = -3;                                   -- nothing on it needs one
    ELSE IF @Current IN ('Pending', 'Approved')
        SET @Result = -4;                                   -- one is already waiting or accepted
    ELSE
    BEGIN
        INSERT INTO Prescriptions (OrderId, CustomerId, ImagePath, DoctorName)
        VALUES (@OrderId, @CustomerId, @ImagePath, @DoctorName);
        SET @Result = 1;
    END

    COMMIT TRANSACTION;
    SELECT @Result;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;";

            try
            {
                int result = _db.ExecuteScalarInt(sql,
                    DbHelper.P("@OrderId", orderId),
                    DbHelper.P("@CustomerId", customerId),
                    DbHelper.P("@ImagePath", storedPath),
                    DbHelper.P("@DoctorName", string.IsNullOrWhiteSpace(doctorName) ? null : doctorName.Trim()));

                switch (result)
                {
                    case 1:
                        message = "Prescription uploaded. The pharmacy will check it before confirming your order.";
                        return true;
                    case -1:
                        message = "That order could not be found in your account.";
                        break;
                    case -2:
                        message = "This order is no longer waiting, so its prescription cannot be changed.";
                        break;
                    case -3:
                        message = "Nothing on this order needs a prescription.";
                        break;
                    case -4:
                        message = "This order already has a prescription that is waiting for the pharmacy or has been approved.";
                        break;
                    default:
                        message = "The prescription was not saved. Please try again.";
                        break;
                }

                DeleteStoredImage(storedFullPath);
                return false;
            }
            catch (Exception ex)
            {
                DeleteStoredImage(storedFullPath);
                message = DbHelper.Describe(ex);
                return false;
            }
        }

        // =====================================================================
        //  PHARMACY OWNER: VERIFICATION QUEUE
        // =====================================================================

        /// <summary>
        /// Requirement 16: the pharmacy's own verification queue. IsCurrent
        /// marks the row that decides the order; older, superseded rows stay
        /// visible as history.
        /// </summary>
        public DataTable GetQueueForPharmacy(int pharmacyId, string verifyStatus)
        {
            const string sql = @"
SELECT  p.PrescriptionId, p.OrderId, u.FullName AS Customer, p.DoctorName,
        p.ImagePath, p.UploadedAt, p.VerifyStatus, p.RejectReason, o.Status AS OrderStatus,
        o.TotalAmount,
        CASE WHEN p.PrescriptionId = (SELECT MAX(x.PrescriptionId) FROM Prescriptions x
                                      WHERE x.OrderId = p.OrderId)
             THEN 1 ELSE 0 END AS IsCurrent
FROM    Prescriptions p
        INNER JOIN Orders o ON o.OrderId    = p.OrderId
        INNER JOIN Users  u ON u.UserId     = p.CustomerId
WHERE   o.PharmacyId = @PharmacyId
  AND   (@VerifyStatus = '' OR p.VerifyStatus = @VerifyStatus)
ORDER BY CASE WHEN p.VerifyStatus = 'Pending' AND o.Status = 'Placed' THEN 0 ELSE 1 END, p.UploadedAt;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@VerifyStatus", verifyStatus ?? ""));
        }

        /// <summary>
        /// Approve or reject, scoped to the owner's own pharmacy.
        ///
        /// The UPDATE changes nothing unless the prescription is the current one
        /// for its order, is still Pending, and the order is still 'Placed'. That
        /// stops a decision on a cancelled or already dispatched order, and stops
        /// a stale screen flipping an old decision. A reason is stored for a
        /// rejection so the customer knows what to fix; an approval clears it.
        /// Returns false when no row qualified.
        /// </summary>
        public bool SetVerifyStatus(int prescriptionId, int pharmacyId, string newStatus, string rejectReason)
        {
            if (newStatus != "Approved" && newStatus != "Rejected") return false;

            string reason = null;
            if (newStatus == "Rejected" && !string.IsNullOrWhiteSpace(rejectReason))
            {
                reason = rejectReason.Trim();
                if (reason.Length > 200) reason = reason.Substring(0, 200);
            }

            const string sql = @"
UPDATE  p
SET     p.VerifyStatus = @Status,
        p.RejectReason = @Reason
FROM    Prescriptions p
        INNER JOIN Orders o ON o.OrderId = p.OrderId
WHERE   p.PrescriptionId = @Id
  AND   o.PharmacyId     = @PharmacyId
  AND   o.Status         = 'Placed'
  AND   p.VerifyStatus   = 'Pending'
  AND   p.PrescriptionId = (SELECT MAX(x.PrescriptionId) FROM Prescriptions x WHERE x.OrderId = p.OrderId);";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@Status", newStatus),
                DbHelper.P("@Reason", reason),
                DbHelper.P("@Id", prescriptionId),
                DbHelper.P("@PharmacyId", pharmacyId)) == 1;
        }

        /// <summary>Kept for existing callers: a decision without a rejection reason.</summary>
        public bool SetVerifyStatus(int prescriptionId, int pharmacyId, string newStatus)
        {
            return SetVerifyStatus(prescriptionId, pharmacyId, newStatus, null);
        }

        /// <summary>Prescriptions still waiting for a decision on orders that can still be confirmed.</summary>
        public int CountPending(int pharmacyId)
        {
            return _db.ExecuteScalarInt(@"
SELECT  COUNT(*)
FROM    Prescriptions p INNER JOIN Orders o ON o.OrderId = p.OrderId
WHERE   o.PharmacyId = @Id AND o.Status = 'Placed' AND p.VerifyStatus = 'Pending';",
                DbHelper.P("@Id", pharmacyId));
        }

        public DataTable GetForOrder(int orderId)
        {
            return _db.ExecuteTable(@"
SELECT  PrescriptionId, OrderId, ImagePath, DoctorName, UploadedAt, VerifyStatus, RejectReason
FROM    Prescriptions WHERE OrderId = @OrderId ORDER BY PrescriptionId DESC;",
                DbHelper.P("@OrderId", orderId));
        }

        /// <summary>True when the order's current (newest) prescription is Approved.</summary>
        public bool OrderHasApprovedPrescription(int orderId)
        {
            return _db.ExecuteScalarString(@"
SELECT TOP 1 VerifyStatus FROM Prescriptions WHERE OrderId = @Id ORDER BY PrescriptionId DESC;",
                DbHelper.P("@Id", orderId)) == "Approved";
        }

        // =====================================================================
        //  RESOLVING A STORED PATH
        // =====================================================================

        /// <summary>
        /// The absolute path of a stored image, for the picture box on the verify
        /// screen, or "" when the stored value is not acceptable.
        ///
        /// ImagePath comes from the database, so it is treated as untrusted: an
        /// absolute path, a UNC share, a drive-relative path or any ".." segment
        /// is refused outright, and whatever is left must resolve to a file inside
        /// the configured upload folder. Without this, a tampered row could make
        /// the owner's machine open any file it can reach.
        /// Relative values are tried against AppContext.BaseDirectory first (the
        /// normal "Uploads\Prescriptions\x.jpg" form) and then against the upload
        /// folder itself (a bare file name).
        /// </summary>
        public static string ResolveImagePath(string storedRelativePath)
        {
            if (!IsSafeRelativePath(storedRelativePath)) return "";

            string folder = UploadFolderFullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            + Path.DirectorySeparatorChar;

            foreach (string root in new[] { AppContext.BaseDirectory, folder })
            {
                string candidate;
                try
                {
                    candidate = Path.GetFullPath(Path.Combine(root, storedRelativePath));
                }
                catch
                {
                    return "";
                }

                if (candidate.StartsWith(folder, StringComparison.OrdinalIgnoreCase) && File.Exists(candidate))
                    return candidate;
            }

            return "";
        }

        /// <summary>No rooted, UNC, drive-relative or parent-directory paths, and no invalid characters.</summary>
        private static bool IsSafeRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return false;
            if (Path.IsPathRooted(path)) return false;                      // C:\..., \..., \\server\...
            if (path.Contains(':')) return false;                           // C:foo, alternate data streams
            if (path.StartsWith("\\") || path.StartsWith("/")) return false;

            string[] segments = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            return !segments.Any(s => s == ".." || s == ".");
        }
    }
}

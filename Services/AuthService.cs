using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Everything to do with getting into the system: login, registration,
    /// profile editing, password change, and the help-desk "forgot password"
    /// flow (request, temporary password, forced change at the next login).
    ///
    /// There is no separate administrator login. All three roles come through
    /// the same query, and the UserType it returns is what decides which
    /// dashboard opens.
    /// </summary>
    public class AuthService
    {
        private readonly DbHelper _db = new DbHelper();

        /// <summary>Consecutive wrong passwords before the account is locked.</summary>
        public const int MaxFailedLogins = 5;

        /// <summary>How long a locked account stays locked.</summary>
        public const int LockoutMinutes = 15;

        /// <summary>
        /// The one message for an unknown email, a wrong password and a locked
        /// account, so the login screen cannot be used to find out which emails
        /// are registered. It states the lockout rule, so a genuine user who
        /// has been locked out still knows to wait.
        /// </summary>
        /// (The numbers are written out because C# cannot build a const string
        /// from int constants; keep them in step with MaxFailedLogins and
        /// LockoutMinutes above.)
        public const string BadCredentialsMessage =
            "Email or password is incorrect. After 5 wrong attempts in a row an account is locked for 15 minutes.";

        // ---------------------------------------------------------------------
        //  LOGIN
        // ---------------------------------------------------------------------

        /// <summary>
        /// Looks the account up by email, then verifies the typed password
        /// against the stored salt and hash in memory. Returns null, with a
        /// message for the user in failureReason, when the login is refused.
        ///
        /// Hardening:
        ///  - an unknown email and a wrong password get the same message;
        ///  - five wrong passwords in a row lock the account for fifteen
        ///    minutes (Users.FailedLoginCount / Users.LockoutUntil), and a
        ///    locked account is refused before the password is even checked;
        ///  - a correct password resets the counter;
        ///  - a legacy SHA-256 hash is replaced by a PBKDF2 hash on the first
        ///    successful login, with a brand new salt.
        ///
        /// The LEFT JOIN on Pharmacies is what supplies PharmacyId for a
        /// pharmacy owner. It is NULL for a SuperAdmin and for a Customer,
        /// because neither of them owns a shop; an Admin without a pharmacy row
        /// is refused, because every owner screen filters on that id.
        /// </summary>
        public User Login(string email, string password, out string failureReason)
        {
            failureReason = "";

            const string sql = @"
SELECT  u.UserId, u.FullName, u.Email, u.PasswordHash, u.PasswordSalt,
        u.Phone, u.Address, u.UserType, u.Status, u.CreatedAt,
        u.FailedLoginCount, u.MustChangePassword,
        CASE WHEN u.LockoutUntil > SYSDATETIME()
             THEN DATEDIFF(SECOND, SYSDATETIME(), u.LockoutUntil) ELSE 0 END AS LockoutSeconds,
        p.PharmacyId, p.PharmacyName, p.Status AS PharmacyStatus
FROM    Users u
        LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE   u.Email = @Email;";

            DataTable table = _db.ExecuteTable(sql, DbHelper.P("@Email", (email ?? "").Trim()));

            if (table.Rows.Count == 0)
            {
                // Spend the same PBKDF2 work a real account would cost, so the
                // response time does not reveal that the email is unknown.
                PasswordHelper.Hash(password, "AAAAAAAAAAAAAAAAAAAAAA==");
                failureReason = BadCredentialsMessage;
                return null;
            }

            DataRow row = table.Rows[0];
            int userId = DbHelper.GetInt(row, "UserId");

            int lockoutSeconds = DbHelper.GetInt(row, "LockoutSeconds");
            if (lockoutSeconds > 0)
            {
                // Same message and the same PBKDF2 work as an unknown email or a
                // wrong password: a distinct "locked" reply, or a faster one,
                // would confirm that the email belongs to a real account.
                PasswordHelper.Hash(password, "AAAAAAAAAAAAAAAAAAAAAA==");
                failureReason = BadCredentialsMessage;
                return null;
            }

            string salt = DbHelper.GetString(row, "PasswordSalt");
            string hash = DbHelper.GetString(row, "PasswordHash");

            if (!PasswordHelper.Verify(password, salt, hash))
            {
                // The counter still advances and locks the account on the fifth
                // failure, but the reply stays the generic one for the same reason.
                RecordFailedLogin(userId);
                failureReason = BadCredentialsMessage;
                return null;
            }

            RecordSuccessfulLogin(userId, password, hash);

            string status = DbHelper.GetString(row, "Status");
            string userType = DbHelper.GetString(row, "UserType");

            // A pharmacy owner's shop is checked first, so a rejected
            // registration gets its own explanation rather than the generic
            // "suspended" message its owner account also carries.
            if (userType == "Admin")
            {
                if (DbHelper.GetInt(row, "PharmacyId") == 0)
                {
                    failureReason = "This pharmacy owner account has no pharmacy on record. Contact the Super Admin.";
                    return null;
                }

                string pharmacyStatus = DbHelper.GetString(row, "PharmacyStatus");
                if (pharmacyStatus == "Rejected")
                {
                    failureReason = "Your pharmacy registration was rejected by the Super Admin.";
                    return null;
                }
                if (pharmacyStatus == "Pending")
                {
                    failureReason = "Your pharmacy registration has not been approved yet.";
                    return null;
                }
                if (pharmacyStatus == "Suspended")
                {
                    failureReason = "Your pharmacy has been suspended by the Super Admin.";
                    return null;
                }
            }

            if (status == "Pending")
            {
                failureReason = "This account is still waiting for Super Admin approval.";
                return null;
            }
            if (status == "Suspended")
            {
                failureReason = "This account has been suspended by the Super Admin.";
                return null;
            }

            return new User
            {
                UserId = userId,
                FullName = DbHelper.GetString(row, "FullName"),
                Email = DbHelper.GetString(row, "Email"),
                Phone = DbHelper.GetString(row, "Phone"),
                Address = DbHelper.GetString(row, "Address"),
                UserType = userType,
                Status = status,
                CreatedAt = DbHelper.GetDate(row, "CreatedAt"),
                PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                PharmacyName = DbHelper.GetString(row, "PharmacyName"),
                // Read here, acted on by LoginForm: the password was right, so
                // this is a real login, but no dashboard may open until the
                // temporary password has been replaced.
                MustChangePassword = DbHelper.GetBool(row, "MustChangePassword")
            };
        }

        /// <summary>
        /// Adds one to the failure counter in a single UPDATE, so two wrong
        /// attempts arriving at once are both counted. On the fifth failure the
        /// account is locked and the counter starts again from zero, so the next
        /// lockout also needs five fresh failures.
        /// </summary>
        private string RecordFailedLogin(int userId)
        {
            const string sql = @"
UPDATE  Users
SET     FailedLoginCount = CASE WHEN FailedLoginCount + 1 >= @Max THEN 0 ELSE FailedLoginCount + 1 END,
        LockoutUntil     = CASE WHEN FailedLoginCount + 1 >= @Max
                                THEN DATEADD(MINUTE, @Minutes, SYSDATETIME()) ELSE LockoutUntil END
OUTPUT  CASE WHEN inserted.LockoutUntil > SYSDATETIME() THEN 1 ELSE 0 END
WHERE   UserId = @UserId;";

            int locked = _db.ExecuteScalarInt(sql,
                DbHelper.P("@Max", MaxFailedLogins),
                DbHelper.P("@Minutes", LockoutMinutes),
                DbHelper.P("@UserId", userId));

            return locked == 1 ? LockoutMessage(LockoutMinutes * 60) : BadCredentialsMessage;
        }

        /// <summary>
        /// Clears the failure counter and, when the stored hash is in an old
        /// format, replaces it with a PBKDF2 hash under a new salt. The hash is
        /// only replaced if it is still the one that was just verified, so a
        /// password change made in the meantime is never overwritten.
        /// </summary>
        private void RecordSuccessfulLogin(int userId, string password, string verifiedHash)
        {
            bool upgrade = PasswordHelper.NeedsUpgrade(verifiedHash);
            string newSalt = upgrade ? PasswordHelper.CreateSalt() : "";
            string newHash = upgrade ? PasswordHelper.Hash(password, newSalt) : "";

            const string sql = @"
UPDATE  Users
SET     FailedLoginCount = 0,
        LockoutUntil     = NULL,
        PasswordSalt     = CASE WHEN @Upgrade = 1 AND PasswordHash = @OldHash THEN @NewSalt ELSE PasswordSalt END,
        PasswordHash     = CASE WHEN @Upgrade = 1 AND PasswordHash = @OldHash THEN @NewHash ELSE PasswordHash END
WHERE   UserId = @UserId;";

            _db.ExecuteNonQuery(sql,
                DbHelper.P("@Upgrade", upgrade ? 1 : 0),
                DbHelper.P("@OldHash", verifiedHash),
                DbHelper.P("@NewSalt", newSalt),
                DbHelper.P("@NewHash", newHash),
                DbHelper.P("@UserId", userId));
        }

        /// <summary>
        /// Minutes are rounded to the nearest whole minute (never below one):
        /// LockoutUntil is stored to the second and DATEDIFF counts second
        /// boundaries, so a lockout that has just started reads 900 or 901
        /// seconds and must still say "15 minutes", not 16.
        /// </summary>
        private static string LockoutMessage(int secondsLeft)
        {
            int minutes = Math.Max(1, (secondsLeft + 29) / 60);
            return "Too many attempts, try again in " + minutes + " minute" + (minutes == 1 ? "" : "s") + ".";
        }

        /// <summary>
        /// Re-reads the logged-in user's status, and for a pharmacy owner the
        /// pharmacy's status too. Login checks these once; the dashboards call
        /// this again before opening a screen, so an account the Super Admin
        /// suspends mid-session is signed out at the next click instead of
        /// keeping full access until it logs out by itself.
        /// Returns false, with a message for the user, when the session must end.
        /// </summary>
        public bool CheckSessionStillValid(out string message)
        {
            message = "";
            if (UserSession.UserId == 0)
            {
                message = "You are not signed in.";
                return false;
            }

            const string sql = @"
SELECT  u.Status, u.UserType, p.Status AS PharmacyStatus
FROM    Users u
        LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE   u.UserId = @UserId;";

            DataTable table = _db.ExecuteTable(sql, DbHelper.P("@UserId", UserSession.UserId));
            if (table.Rows.Count == 0)
            {
                message = "This account no longer exists. You have been signed out.";
                return false;
            }

            string userType = DbHelper.GetString(table.Rows[0], "UserType");
            if (userType != UserSession.UserType)
            {
                message = "The role on this account has changed. Please sign in again.";
                return false;
            }

            string status = DbHelper.GetString(table.Rows[0], "Status");
            if (status != "Active")
            {
                message = "Your account is no longer active (status: " + status + "). You have been signed out.";
                return false;
            }

            if (UserSession.IsAdmin)
            {
                string pharmacyStatus = DbHelper.GetString(table.Rows[0], "PharmacyStatus");
                if (pharmacyStatus != "Approved")
                {
                    message = "Your pharmacy is no longer approved (status: " +
                              (pharmacyStatus.Length == 0 ? "missing" : pharmacyStatus) +
                              "). You have been signed out.";
                    return false;
                }
            }

            return true;
        }

        // ---------------------------------------------------------------------
        //  REGISTRATION
        // ---------------------------------------------------------------------

        public bool EmailExists(string email)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Users WHERE Email = @Email;",
                DbHelper.P("@Email", email.Trim())) > 0;
        }

        public bool PhoneExists(string phone)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Users WHERE Phone = @Phone;",
                DbHelper.P("@Phone", phone.Trim())) > 0;
        }

        public bool LicenseExists(string licenseNo)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Pharmacies WHERE LicenseNo = @LicenseNo;",
                DbHelper.P("@LicenseNo", licenseNo.Trim())) > 0;
        }

        /// <summary>
        /// Registers a customer. A customer is created Active and can use the
        /// platform immediately.
        /// </summary>
        public int RegisterCustomer(User user, string password)
        {
            string salt = PasswordHelper.CreateSalt();
            string hash = PasswordHelper.Hash(password, salt);

            const string sql = @"
INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Phone, Address, UserType, Status)
VALUES (@FullName, @Email, @Hash, @Salt, @Phone, @Address, 'Customer', 'Active');
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return _db.ExecuteScalarInt(sql,
                DbHelper.P("@FullName", user.FullName.Trim()),
                DbHelper.P("@Email", user.Email.Trim()),
                DbHelper.P("@Hash", hash),
                DbHelper.P("@Salt", salt),
                DbHelper.P("@Phone", user.Phone.Trim()),
                DbHelper.P("@Address", user.Address));
        }

        /// <summary>
        /// Registers a pharmacy owner. The Users row and the Pharmacies row are
        /// written inside one transaction, both with Status 'Pending', so the
        /// application can never end up with an owner who has no shop or a shop
        /// that has no owner. Neither becomes usable until the Super Admin
        /// approves the registration.
        ///
        /// This method opens its own connection, so a SqlException from it has
        /// not passed through DbHelper; it is wrapped in a DataAccessException
        /// carrying DbHelper.Describe's sentence, with the SqlException kept as
        /// InnerException so the form can still tell which UNIQUE constraint a
        /// racing duplicate hit.
        /// </summary>
        public int RegisterPharmacyOwner(User owner, Pharmacy pharmacy, string password)
        {
            string salt = PasswordHelper.CreateSalt();
            string hash = PasswordHelper.Hash(password, salt);

            try
            {
                using (SqlConnection conn = _db.GetConnection())
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            int newUserId;

                            const string insertUser = @"
INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Phone, Address, UserType, Status)
VALUES (@FullName, @Email, @Hash, @Salt, @Phone, @Address, 'Admin', 'Pending');
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            using (SqlCommand cmd = new SqlCommand(insertUser, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@FullName", owner.FullName.Trim());
                                cmd.Parameters.AddWithValue("@Email", owner.Email.Trim());
                                cmd.Parameters.AddWithValue("@Hash", hash);
                                cmd.Parameters.AddWithValue("@Salt", salt);
                                cmd.Parameters.AddWithValue("@Phone", owner.Phone.Trim());
                                cmd.Parameters.AddWithValue("@Address", (object)owner.Address ?? DBNull.Value);
                                newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            const string insertPharmacy = @"
INSERT INTO Pharmacies (OwnerId, PharmacyName, LicenseNo, Area, Address, ContactPhone, LogoPath, Status)
VALUES (@OwnerId, @Name, @License, @Area, @Address, @Phone, @Logo, 'Pending');";

                            using (SqlCommand cmd = new SqlCommand(insertPharmacy, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@OwnerId", newUserId);
                                cmd.Parameters.AddWithValue("@Name", pharmacy.PharmacyName.Trim());
                                cmd.Parameters.AddWithValue("@License", pharmacy.LicenseNo.Trim());
                                cmd.Parameters.AddWithValue("@Area", pharmacy.Area.Trim());
                                cmd.Parameters.AddWithValue("@Address", pharmacy.Address.Trim());
                                cmd.Parameters.AddWithValue("@Phone", pharmacy.ContactPhone.Trim());
                                cmd.Parameters.AddWithValue("@Logo",
                                    string.IsNullOrWhiteSpace(pharmacy.LogoPath) ? (object)DBNull.Value : pharmacy.LogoPath);
                                cmd.ExecuteNonQuery();
                            }

                            tx.Commit();
                            return newUserId;
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch (InvalidOperationException) { /* already rolled back by the server */ }
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DataAccessException(DbHelper.Describe(ex), ex);
            }
        }

        // ---------------------------------------------------------------------
        //  PROFILE AND PASSWORD
        // ---------------------------------------------------------------------

        public User GetUser(int userId)
        {
            const string sql = @"
SELECT  u.UserId, u.FullName, u.Email, u.Phone, u.Address, u.UserType,
        u.Status, u.CreatedAt, p.PharmacyId, p.PharmacyName
FROM    Users u
        LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE   u.UserId = @UserId;";

            DataTable table = _db.ExecuteTable(sql, DbHelper.P("@UserId", userId));
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            return new User
            {
                UserId = DbHelper.GetInt(row, "UserId"),
                FullName = DbHelper.GetString(row, "FullName"),
                Email = DbHelper.GetString(row, "Email"),
                Phone = DbHelper.GetString(row, "Phone"),
                Address = DbHelper.GetString(row, "Address"),
                UserType = DbHelper.GetString(row, "UserType"),
                Status = DbHelper.GetString(row, "Status"),
                CreatedAt = DbHelper.GetDate(row, "CreatedAt"),
                PharmacyId = DbHelper.GetInt(row, "PharmacyId"),
                PharmacyName = DbHelper.GetString(row, "PharmacyName")
            };
        }

        /// <summary>Email is deliberately not editable: it is the login identifier.</summary>
        public bool UpdateProfile(int userId, string fullName, string phone, string address)
        {
            const string sql = @"
UPDATE  Users
SET     FullName = @FullName, Phone = @Phone, Address = @Address
WHERE   UserId = @UserId;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@FullName", fullName.Trim()),
                DbHelper.P("@Phone", phone.Trim()),
                DbHelper.P("@Address", address),
                DbHelper.P("@UserId", userId)) == 1;
        }

        /// <summary>
        /// Changes a password. The stored salt and hash are read, the current
        /// password is verified against them in memory (PBKDF2 or legacy
        /// format), and only then is the new PBKDF2 hash written under a fresh
        /// salt. The UPDATE is conditional on PasswordHash still being the value
        /// that was read, so if the password was changed somewhere else in
        /// between, no row is updated and the method returns false instead of
        /// silently overwriting that change. False therefore means "wrong
        /// current password, unknown user, or changed concurrently".
        ///
        /// The same UPDATE clears MustChangePassword: a user who replaces a
        /// temporary password from My Account has done exactly what the forced
        /// change at login asks for, so they must not be asked again.
        /// </summary>
        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            DataTable table = _db.ExecuteTable(
                "SELECT PasswordSalt, PasswordHash FROM Users WHERE UserId = @UserId;",
                DbHelper.P("@UserId", userId));

            if (table.Rows.Count == 0) return false;

            string currentSalt = DbHelper.GetString(table.Rows[0], "PasswordSalt");
            string storedHash = DbHelper.GetString(table.Rows[0], "PasswordHash");

            if (!PasswordHelper.Verify(currentPassword, currentSalt, storedHash)) return false;

            string newSalt = PasswordHelper.CreateSalt();
            string newHash = PasswordHelper.Hash(newPassword, newSalt);

            const string sql = @"
UPDATE  Users
SET     PasswordHash = @NewHash, PasswordSalt = @NewSalt,
        FailedLoginCount = 0, LockoutUntil = NULL,
        MustChangePassword = 0
WHERE   UserId = @UserId AND PasswordHash = @OldStoredHash;";

            return _db.ExecuteNonQuery(sql,
                DbHelper.P("@NewHash", newHash),
                DbHelper.P("@NewSalt", newSalt),
                DbHelper.P("@UserId", userId),
                DbHelper.P("@OldStoredHash", storedHash)) == 1;
        }

        // ---------------------------------------------------------------------
        //  FORGOTTEN PASSWORD (help-desk reset through the Super Admin)
        //
        //  PharmaLink has no email or SMS service, so a reset cannot be mailed
        //  out. Instead:
        //   1. the user asks from the login screen, proving nothing more than
        //      that they know the account's email AND registered mobile number;
        //   2. the Super Admin sees the request on Manage Users, phones that
        //      registered number, and issues a random temporary password;
        //   3. the next login with it is forced to choose a new password
        //      before any dashboard opens.
        // ---------------------------------------------------------------------

        /// <summary>
        /// The one reply to every reset request, matched or not, so the form
        /// cannot be used to find out which email and mobile pairs are real.
        /// </summary>
        public const string PasswordResetRequestedMessage =
            "If those details match an account, your request has been sent to the PharmaLink administrator, " +
            "who will contact you on your registered mobile number with a temporary password.";

        /// <summary>
        /// Records a "forgot password" request and always returns
        /// PasswordResetRequestedMessage.
        ///
        /// Exactly one parameterised UPDATE runs whatever was typed, so a match
        /// and a miss cost the same round trip and the response time gives
        /// nothing away. It only touches an Active or Suspended Admin or
        /// Customer whose Email and Phone BOTH match: a Pending registration
        /// has nothing to reset yet, and the Super Admin account is never reset
        /// through a form anyone can reach from the login screen.
        ///
        /// A request that is already waiting keeps its original timestamp
        /// (ISNULL), so pressing Submit again cannot push an old request down
        /// the Super Admin's list or make it look newer than it is.
        /// </summary>
        public string RequestPasswordReset(string email, string mobile)
        {
            const string sql = @"
UPDATE  Users
SET     PasswordResetRequestedAt = ISNULL(PasswordResetRequestedAt, SYSDATETIME())
WHERE   Email = @Email
  AND   Phone = @Phone
  AND   UserType IN ('Admin', 'Customer')
  AND   Status   IN ('Active', 'Suspended');";

            _db.ExecuteNonQuery(sql,
                DbHelper.P("@Email", (email ?? "").Trim()),
                DbHelper.P("@Phone", (mobile ?? "").Trim()));

            // The row count is deliberately ignored: the caller must not be
            // able to tell a match from a miss.
            return PasswordResetRequestedMessage;
        }

        /// <summary>Reset requests still waiting for the Super Admin, for the dashboard.</summary>
        public int CountPendingPasswordResets()
        {
            return _db.ExecuteScalarInt(@"
SELECT COUNT(*) FROM Users
WHERE  PasswordResetRequestedAt IS NOT NULL AND UserType IN ('Admin', 'Customer');");
        }

        /// <summary>
        /// Letters and digits a person can read aloud over the phone without
        /// confusion: no I, l, i, O, o, 0 or 1.
        /// </summary>
        private const string TempLetters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";
        private const string TempDigits = "23456789";
        private const int TempPasswordLength = 10;

        /// <summary>
        /// Super Admin only. Replaces an Admin's or Customer's password with a
        /// random temporary one and returns it in tempPassword, which the caller
        /// shows once and never stores or logs.
        ///
        /// One UPDATE, so nothing can be half done: the new hash under a brand
        /// new salt, MustChangePassword = 1 (the next login must replace it),
        /// the reset request cleared, and the lockout lifted so a user who
        /// locked themselves out guessing can sign in with it straight away.
        /// The WHERE clause refuses the SuperAdmin row even if a caller passes
        /// its id. Returns false (tempPassword empty) when no row was changed.
        /// </summary>
        public bool IssueTemporaryPassword(int userId, out string tempPassword)
        {
            string candidate = GenerateTemporaryPassword();
            string salt = PasswordHelper.CreateSalt();
            string hash = PasswordHelper.Hash(candidate, salt);

            const string sql = @"
UPDATE  Users
SET     PasswordHash             = @Hash,
        PasswordSalt             = @Salt,
        MustChangePassword       = 1,
        PasswordResetRequestedAt = NULL,
        FailedLoginCount         = 0,
        LockoutUntil             = NULL
WHERE   UserId = @UserId
  AND   UserType IN ('Admin', 'Customer');";

            int changed = _db.ExecuteNonQuery(sql,
                DbHelper.P("@Hash", hash),
                DbHelper.P("@Salt", salt),
                DbHelper.P("@UserId", userId));

            tempPassword = changed == 1 ? candidate : "";
            return changed == 1;
        }

        /// <summary>
        /// Ten characters from the unambiguous alphabet, drawn with the
        /// cryptographic RandomNumberGenerator (System.Random is predictable).
        /// A draw without at least one letter and one digit is thrown away and
        /// drawn again - rather than forcing a character into a fixed position -
        /// so every accepted password is equally likely and it always satisfies
        /// Validator.IsStrongPassword.
        /// </summary>
        private static string GenerateTemporaryPassword()
        {
            string alphabet = TempLetters + TempDigits;
            char[] chars = new char[TempPasswordLength];

            while (true)
            {
                for (int i = 0; i < chars.Length; i++)
                    chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

                string candidate = new string(chars);
                if (candidate.Any(char.IsLetter) && candidate.Any(char.IsDigit))
                {
                    Array.Clear(chars);
                    return candidate;
                }
            }
        }

        /// <summary>
        /// The forced change at login. Verifies the temporary password again,
        /// then writes the new hash under a fresh salt and clears
        /// MustChangePassword, all in ChangePassword's single UPDATE that is
        /// guarded by the old hash - so if the Super Admin issued yet another
        /// temporary password in the meantime, nothing is overwritten.
        ///
        /// The rules are checked here as well as on the form (at least 8
        /// characters with a letter and a digit, and not the temporary password
        /// itself), so no caller can clear the flag with a weak password.
        /// False means a rule failed, the temporary password no longer matches,
        /// or the account is gone.
        /// </summary>
        public bool CompleteRequiredPasswordChange(int userId, string currentTempPassword, string newPassword)
        {
            if (!Validator.IsStrongPassword(newPassword)) return false;
            if (newPassword == currentTempPassword) return false;

            return ChangePassword(userId, currentTempPassword, newPassword);
        }

        // ---------------------------------------------------------------------
        //  SUPER ADMIN: USER LIST
        // ---------------------------------------------------------------------

        /// <summary>
        /// Requirement 4. Every Admin and Customer in one grid, with the shop
        /// name and its status filled in beside an owner's row through a LEFT
        /// JOIN. An empty keyword or status means "no filter" rather than
        /// "no results".
        ///
        /// PasswordResetRequestedAt is returned (NULL when no request is
        /// waiting) and waiting requests sort to the top, oldest first, so the
        /// Super Admin sees the queue without hunting for it.
        /// resetRequestedOnly narrows the list to those requests.
        /// </summary>
        public DataTable SearchUsers(string keyword, string status, string userType, bool resetRequestedOnly = false)
        {
            const string sql = @"
SELECT  u.UserId, u.FullName, u.Email, u.Phone, u.UserType, u.Status,
        ISNULL(p.PharmacyName, '-') AS PharmacyName,
        ISNULL(p.Status, '-')       AS PharmacyStatus,
        u.CreatedAt,
        u.PasswordResetRequestedAt
FROM    Users u
        LEFT JOIN Pharmacies p ON p.OwnerId = u.UserId
WHERE   u.UserType <> 'SuperAdmin'
  AND   (@Keyword  = '' OR u.FullName LIKE '%' + @Keyword + '%' OR u.Email LIKE '%' + @Keyword + '%')
  AND   (@Status   = '' OR u.Status   = @Status)
  AND   (@UserType = '' OR u.UserType = @UserType)
  AND   (@ResetOnly = 0 OR u.PasswordResetRequestedAt IS NOT NULL)
ORDER BY CASE WHEN u.PasswordResetRequestedAt IS NULL THEN 1 ELSE 0 END,
         u.PasswordResetRequestedAt, u.UserType, u.FullName;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@Keyword", keyword ?? ""),
                DbHelper.P("@Status", status ?? ""),
                DbHelper.P("@UserType", userType ?? ""),
                DbHelper.P("@ResetOnly", resetRequestedOnly ? 1 : 0));
        }

        /// <summary>
        /// Suspends or activates an Admin or Customer account. Returns true when
        /// something changed; message always says what happened (or why not).
        ///
        /// For a pharmacy owner the account and the shop must not disagree, so
        /// everything runs in one transaction:
        ///  - suspending an owner also suspends an Approved pharmacy;
        ///  - activating an owner whose pharmacy is Suspended reinstates it
        ///    (the same effect as Reinstate on Manage Pharmacies);
        ///  - activating an owner whose pharmacy is Pending or Rejected is
        ///    refused, because that is a licence decision that belongs on
        ///    Manage Pharmacies, not an account switch.
        /// Medicines are never touched: customers only see medicines of an
        /// Approved pharmacy, so the shop status alone hides or shows them.
        /// </summary>
        public bool SetUserStatus(int userId, string newStatus, out string message)
        {
            if (newStatus != "Active" && newStatus != "Suspended")
            {
                message = "An account can only be set to Active or Suspended here.";
                return false;
            }

            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @UserType NVARCHAR(15), @OldStatus NVARCHAR(15),
        @PharmacyId INT, @PharmacyStatus NVARCHAR(15),
        @Outcome NVARCHAR(30) = 'NoChange', @UsersChanged INT = 0, @PharmacyChanged INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    SELECT  @UserType = u.UserType, @OldStatus = u.Status
    FROM    Users u WITH (UPDLOCK, HOLDLOCK)
    WHERE   u.UserId = @UserId;

    SELECT  @PharmacyId = p.PharmacyId, @PharmacyStatus = p.Status
    FROM    Pharmacies p WITH (UPDLOCK, HOLDLOCK)
    WHERE   p.OwnerId = @UserId;

    IF @UserType IS NULL OR @UserType = 'SuperAdmin'
        SET @Outcome = 'NotFound';
    ELSE IF @UserType = 'Admin' AND @NewStatus = 'Active'
            AND (@PharmacyId IS NULL OR @PharmacyStatus IN ('Pending', 'Rejected'))
        SET @Outcome = 'PharmacyNotApproved';
    ELSE
    BEGIN
        UPDATE Users SET Status = @NewStatus WHERE UserId = @UserId AND Status <> @NewStatus;
        SET @UsersChanged = @@ROWCOUNT;

        IF @UserType = 'Admin' AND @NewStatus = 'Suspended' AND @PharmacyStatus = 'Approved'
        BEGIN
            UPDATE Pharmacies SET Status = 'Suspended' WHERE PharmacyId = @PharmacyId AND Status = 'Approved';
            SET @PharmacyChanged = @@ROWCOUNT;
        END
        ELSE IF @UserType = 'Admin' AND @NewStatus = 'Active' AND @PharmacyStatus = 'Suspended'
        BEGIN
            UPDATE Pharmacies SET Status = 'Approved' WHERE PharmacyId = @PharmacyId AND Status = 'Suspended';
            SET @PharmacyChanged = @@ROWCOUNT;
        END

        IF @UsersChanged + @PharmacyChanged > 0 SET @Outcome = 'Changed';
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT @Outcome AS Outcome, @UserType AS UserType, @OldStatus AS OldStatus,
       @PharmacyStatus AS PharmacyStatus, @PharmacyChanged AS PharmacyChanged;";

            DataTable table = _db.ExecuteTable(sql,
                DbHelper.P("@UserId", userId),
                DbHelper.P("@NewStatus", newStatus));

            DataRow row = table.Rows[0];
            string outcome = DbHelper.GetString(row, "Outcome");
            string pharmacyStatus = DbHelper.GetString(row, "PharmacyStatus");
            bool pharmacyChanged = DbHelper.GetInt(row, "PharmacyChanged") > 0;

            switch (outcome)
            {
                case "NotFound":
                    message = DbHelper.GetString(row, "UserType") == "SuperAdmin"
                        ? "The Super Admin account cannot be suspended or activated here."
                        : "That account no longer exists.";
                    return false;

                case "PharmacyNotApproved":
                    message = pharmacyStatus.Length == 0
                        ? "This pharmacy owner has no pharmacy on record, so the account cannot be activated."
                        : "This owner's pharmacy is " + pharmacyStatus + ". Approve the pharmacy from Manage Pharmacies " +
                          "instead - approving it activates the owner's account at the same time.";
                    return false;

                case "Changed":
                    if (newStatus == "Suspended")
                        message = pharmacyChanged
                            ? "The account and its pharmacy are now Suspended. The shop is hidden from customers."
                            : "The account is now Suspended.";
                    else
                        message = pharmacyChanged
                            ? "The account is now Active and its pharmacy has been reinstated."
                            : "The account is now Active.";
                    return true;

                default:
                    message = "Nothing changed - the account is already " + newStatus + ".";
                    return false;
            }
        }
    }
}

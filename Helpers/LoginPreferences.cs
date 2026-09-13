using System.Security;

namespace PharmaLinkApp.Helpers
{
    /// <summary>
    /// "Remember my email" on the login screen.
    ///
    /// Only the email address is ever written, as one line of plain text in
    /// %LOCALAPPDATA%\PharmaLink\login-preferences.txt. A password is never
    /// stored, not even hashed: the file lives outside the database's
    /// protection, and anyone who can read the user's profile folder could
    /// otherwise walk straight in. An email address is not a secret - it is
    /// already shown on the login screen as it is typed.
    ///
    /// LocalApplicationData (not Roaming) keeps the preference on this
    /// computer only, which is what "remember me on this PC" means.
    ///
    /// This is a convenience, so it must never stop anyone signing in. Every
    /// file system failure (a read-only profile, a locked or deleted file, a
    /// policy that blocks the folder) is swallowed: Load then simply returns an
    /// empty string and Save / Clear do nothing.
    /// </summary>
    public static class LoginPreferences
    {
        private static string FolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PharmaLink");

        private static string FilePath => Path.Combine(FolderPath, "login-preferences.txt");

        /// <summary>
        /// The remembered email, or "" when nothing is remembered or the file
        /// cannot be read. A value that is not a valid email (a hand-edited or
        /// damaged file) is ignored rather than pre-filled into the login box.
        /// </summary>
        public static string Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return "";

                string email = (File.ReadLines(FilePath).FirstOrDefault() ?? "").Trim();
                return Validator.IsEmail(email) ? email : "";
            }
            catch (Exception ex) when (IsFileSystemProblem(ex))
            {
                return "";
            }
        }

        /// <summary>Remembers this email for the next launch, creating the folder when needed.</summary>
        public static void Save(string email)
        {
            if (!Validator.IsEmail(email))
            {
                Clear();
                return;
            }

            try
            {
                Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, email.Trim() + Environment.NewLine);
            }
            catch (Exception ex) when (IsFileSystemProblem(ex))
            {
                // Not remembering the email is harmless; the user types it next time.
            }
        }

        /// <summary>Forgets the remembered email. Does nothing when there is none.</summary>
        public static void Clear()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception ex) when (IsFileSystemProblem(ex))
            {
                // A file that cannot be deleted just keeps pre-filling the email.
            }
        }

        /// <summary>
        /// Only the exceptions the file system can raise are swallowed; a genuine
        /// bug (a NullReferenceException, say) still reaches the global handler.
        /// </summary>
        private static bool IsFileSystemProblem(Exception ex)
        {
            return ex is IOException
                || ex is UnauthorizedAccessException
                || ex is SecurityException
                || ex is NotSupportedException
                || ex is ArgumentException;
        }
    }
}

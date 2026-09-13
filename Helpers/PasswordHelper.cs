using System.Security.Cryptography;
using System.Text;

namespace PharmaLinkApp.Helpers
{
    /// <summary>
    /// Passwords are never stored or logged in plain text.
    ///
    /// Every user gets a random salt of their own (Users.PasswordSalt, Base64),
    /// and what goes into Users.PasswordHash is
    ///
    ///     PBKDF2$100000$Base64( PBKDF2-HMAC-SHA256( password, salt, 100000, 32 bytes ) )
    ///
    /// A single SHA-256 is fast by design, which is exactly wrong for passwords:
    /// a leaked table can be brute forced at billions of guesses per second.
    /// PBKDF2 repeats the hash 100,000 times, so every guess costs an attacker
    /// the same 100,000 rounds it costs the login screen once. The iteration
    /// count is written into the stored value, so it can be raised later without
    /// breaking existing accounts.
    ///
    /// Older databases hold legacy hashes, Base64( SHA-256( salt + password ) )
    /// with no prefix. Verify still accepts them, NeedsUpgrade reports them, and
    /// AuthService.Login re-hashes the password in the new format the first time
    /// such an account signs in successfully.
    ///
    /// The seed accounts in PharmaLinkDB_Setup.sql were hashed with exactly this
    /// method, which is why they log in without any extra setup.
    /// </summary>
    public static class PasswordHelper
    {
        private const string Prefix = "PBKDF2$";
        private const int Iterations = 100000;
        private const int KeyBytes = 32;
        private const int LegacyHashBytes = 32;

        // A stored iteration count outside this range is treated as corrupt, so
        // a hand-edited "PBKDF2$2000000000$..." cannot freeze the login screen.
        private const int MinIterations = 1000;
        private const int MaxIterations = 10000000;

        /// <summary>UTF-8 without a BOM that replaces invalid characters instead of throwing.</summary>
        private static readonly Encoding PasswordEncoding = new UTF8Encoding(false, false);

        /// <summary>Creates a fresh 16 byte random salt, Base64 encoded (24 characters).</summary>
        public static string CreateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        }

        /// <summary>Hashes a password with the given salt in the current PBKDF2 format.</summary>
        public static string Hash(string password, string salt)
        {
            byte[] key = Derive(password, SaltBytes(salt), Iterations);
            return Prefix + Iterations + "$" + Convert.ToBase64String(key);
        }

        /// <summary>
        /// True when the typed password produces the stored hash. Accepts both
        /// the PBKDF2 format and the legacy unprefixed SHA-256 format. Both
        /// comparisons run in constant time, so the time taken does not reveal
        /// how many leading bytes of a guess were right.
        /// </summary>
        public static bool Verify(string password, string salt, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            try
            {
                if (storedHash.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    // PBKDF2$<iterations>$<base64 key>
                    string[] parts = storedHash.Split('$');
                    if (parts.Length != 3) return false;
                    if (!int.TryParse(parts[1], out int iterations) ||
                        iterations < MinIterations || iterations > MaxIterations) return false;

                    // The key must be exactly the length this class writes. Without
                    // this, a damaged value such as "PBKDF2$100000$" decoded to zero
                    // bytes, PBKDF2 happily produced zero bytes, and two empty arrays
                    // compare equal - so every password would have been accepted.
                    byte[] expected = Convert.FromBase64String(parts[2]);
                    if (expected.Length != KeyBytes) return false;

                    byte[] actual = Derive(password, SaltBytes(salt), iterations);
                    return CryptographicOperations.FixedTimeEquals(actual, expected);
                }

                // Legacy: Base64( SHA-256( salt + password ) ) on the salt *string*.
                byte[] legacyExpected = Convert.FromBase64String(storedHash);
                if (legacyExpected.Length != LegacyHashBytes) return false;
                byte[] legacyActual = SHA256.HashData(PasswordEncoding.GetBytes((salt ?? string.Empty) + (password ?? string.Empty)));
                return CryptographicOperations.FixedTimeEquals(legacyActual, legacyExpected);
            }
            catch (FormatException)
            {
                // A corrupt stored value can never match anything.
                return false;
            }
        }

        /// <summary>
        /// True when a stored hash is not in the current format (a legacy SHA-256
        /// value, or PBKDF2 with fewer iterations than today's setting), so the
        /// caller should re-hash the password after a successful Verify.
        /// </summary>
        public static bool NeedsUpgrade(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || !storedHash.StartsWith(Prefix, StringComparison.Ordinal))
                return true;

            string[] parts = storedHash.Split('$');
            return parts.Length != 3 || !int.TryParse(parts[1], out int iterations) || iterations < Iterations;
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            // The byte overload with a replacing encoder: a pasted password that
            // contains a lone UTF-16 surrogate made the string overload throw
            // EncoderFallbackException out of Login. For every valid password the
            // bytes are identical to the strict encoder's, so existing hashes and
            // the seed accounts still verify.
            byte[] passwordBytes = PasswordEncoding.GetBytes(password ?? string.Empty);
            return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, HashAlgorithmName.SHA256, KeyBytes);
        }

        /// <summary>
        /// The salt column holds Base64, so the real salt is the decoded bytes.
        /// A value that is not valid Base64 (hand-edited data) falls back to its
        /// UTF-8 bytes rather than making the account impossible to log in to.
        /// </summary>
        private static byte[] SaltBytes(string salt)
        {
            if (string.IsNullOrEmpty(salt)) return Array.Empty<byte>();
            try
            {
                return Convert.FromBase64String(salt);
            }
            catch (FormatException)
            {
                return Encoding.UTF8.GetBytes(salt);
            }
        }
    }
}

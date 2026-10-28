using System.Text.RegularExpressions;

namespace PharmaLinkApp.Helpers
{
    /// <summary>
    /// Every validation rule the forms enforce lives here, in one place, so the
    /// same rule cannot drift between two screens.
    ///
    /// Most rules exist so the user sees a red label under the field before
    /// anything is sent anywhere. Some of them are also backed by the database
    /// (the email pattern by CK_Users_Email, the commission and discount caps by
    /// CK_Pharmacies_Comm and CK_Offers_Percent, duplicate emails and phones by
    /// UNIQUE constraints), so a bypassed form still cannot write those bad rows.
    /// The phone number formats and the password strength rule are enforced
    /// only here: the database cannot see a password, and the Phone columns are
    /// plain NVARCHAR(20) with no CHECK.
    /// </summary>
    public static class Validator
    {
        private static readonly Regex EmailPattern =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled);

        private static readonly Regex DigitsOnly =
            new Regex(@"^\d+$", RegexOptions.Compiled);

        public static bool IsBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Matches the CK_Users_Email CHECK constraint on the Users table.</summary>
        public static bool IsEmail(string value)
        {
            return !IsBlank(value) && EmailPattern.IsMatch(value.Trim());
        }

        /// <summary>Bangladeshi mobile numbers are eleven digits and start with 01.</summary>
        public static bool IsMobile(string value)
        {
            if (IsBlank(value)) return false;
            string digits = value.Trim();
            return digits.Length == 11 && DigitsOnly.IsMatch(digits) && digits.StartsWith("01");
        }

        /// <summary>
        /// A shop's contact number: exactly eleven digits starting with 01 (a
        /// mobile) or 02 (a Dhaka landline such as 02955000021).
        /// </summary>
        public static bool IsContactPhone(string value)
        {
            if (IsBlank(value)) return false;
            string digits = value.Trim();
            return digits.Length == 11 && DigitsOnly.IsMatch(digits) &&
                   (digits.StartsWith("01") || digits.StartsWith("02"));
        }

        /// <summary>At least eight characters, with at least one letter and at least one digit.</summary>
        public static bool IsStrongPassword(string value)
        {
            if (IsBlank(value) || value.Length < 8) return false;
            return value.Any(char.IsLetter) && value.Any(char.IsDigit);
        }

        public static bool IsPositiveDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, out result) && result > 0m;
        }

        public static bool IsNonNegativeInt(string value, out int result)
        {
            return int.TryParse(value, out result) && result >= 0;
        }

        public static bool IsPositiveInt(string value, out int result)
        {
            return int.TryParse(value, out result) && result > 0;
        }

        /// <summary>Commission is a platform rate, capped by CK_Pharmacies_Comm at 30 percent.</summary>
        public static bool IsCommissionRate(string value, out decimal result)
        {
            return decimal.TryParse(value, out result) && result >= 0m && result <= 30m;
        }

        /// <summary>Discounts are capped by CK_Offers_Percent at 70 percent.</summary>
        public static bool IsDiscountPercent(string value, out decimal result)
        {
            return decimal.TryParse(value, out result) && result > 0m && result <= 70m;
        }

        /// <summary>A DGDA licence looks like DGDA-DH-10021.</summary>
        public static bool IsLicenseNo(string value)
        {
            return !IsBlank(value) && value.Trim().Length >= 6;
        }

        public static bool IsFutureDate(DateTime value)
        {
            return value.Date > DateTime.Today;
        }

        // ---------------------------------------------------------------------
        //  PAYMENT DETAILS
        //  Checked before anything is sent to the payment gateway, so a typing
        //  mistake is a red label rather than a declined payment.
        // ---------------------------------------------------------------------

        /// <summary>The card number with spaces and dashes taken out.</summary>
        public static string CardDigits(string value)
        {
            return (value ?? "").Replace(" ", "").Replace("-", "");
        }

        /// <summary>
        /// A Visa card number: starts with 4, 16 digits, and passes the Luhn
        /// check digit that every real card number carries. Spaces and dashes
        /// are allowed, as printed on the card.
        /// </summary>
        public static bool IsVisaCardNumber(string value)
        {
            string digits = CardDigits(value);
            return digits.Length == 16 && DigitsOnly.IsMatch(digits) && digits[0] == '4' && PassesLuhn(digits);
        }

        private static bool PassesLuhn(string digits)
        {
            int sum = 0;
            bool doubleIt = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int d = digits[i] - '0';
                if (doubleIt)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                doubleIt = !doubleIt;
            }
            return sum % 10 == 0;
        }

        /// <summary>
        /// A card expiry written MM/YY that has not passed yet. A card is valid
        /// until the last day of its expiry month.
        /// </summary>
        public static bool IsCardExpiry(string value)
        {
            if (IsBlank(value)) return false;
            Match m = Regex.Match(value.Trim(), @"^(0[1-9]|1[0-2])\s*/\s*(\d{2})$");
            if (!m.Success) return false;

            int month = int.Parse(m.Groups[1].Value);
            int year = 2000 + int.Parse(m.Groups[2].Value);
            DateTime firstOfNextMonth = new DateTime(year, month, 1).AddMonths(1);
            return firstOfNextMonth > DateTime.Today;
        }

        /// <summary>The three digit security code on the back of a Visa card.</summary>
        public static bool IsCvv(string value)
        {
            return !IsBlank(value) && value.Trim().Length == 3 && DigitsOnly.IsMatch(value.Trim());
        }

        /// <summary>A six digit one-time code, as bKash and Nagad send by SMS.</summary>
        public static bool IsOtp(string value)
        {
            return !IsBlank(value) && value.Trim().Length == 6 && DigitsOnly.IsMatch(value.Trim());
        }

        /// <summary>A wallet PIN: five digits for bKash, four for Nagad.</summary>
        public static bool IsWalletPin(string value, int length)
        {
            return !IsBlank(value) && value.Length == length && DigitsOnly.IsMatch(value);
        }
    }
}

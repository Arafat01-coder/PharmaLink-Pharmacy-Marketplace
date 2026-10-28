using System.Security.Cryptography;
using PharmaLinkApp.Helpers;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// What the gateway hands back when it has approved a payment. Checkout
    /// writes it into the Payments row inside the order transaction.
    /// Only the last four digits of a card ever leave this object.
    /// </summary>
    public class PaymentAuthorization
    {
        public string Method { get; set; } = "";          // bKash | Nagad | Card
        public string TransactionRef { get; set; } = "";
        public string WalletNumber { get; set; } = "";    // bKash / Nagad only
        public string CardLast4 { get; set; } = "";       // Card only
        public decimal Amount { get; set; }
        public DateTime AuthorizedAt { get; set; }
    }

    /// <summary>
    /// The online payment step of checkout: bKash and Nagad (wallet number,
    /// one-time code, PIN) and Visa card (number, expiry, CVV).
    ///
    /// The checkout never talks to a payment provider directly, only to this
    /// interface. A live bKash Tokenized Checkout, Nagad or card gateway such as
    /// SSLCommerz needs a merchant account and API keys, so the application
    /// ships with <see cref="SandboxPaymentGateway"/>, which runs the same
    /// steps and the same checks without moving money. Swapping in a real
    /// provider means writing one more class that implements these three
    /// methods and returning it from <see cref="PaymentGateways.Current"/>.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>True when this gateway moves real money; the payment screen says so either way.</summary>
        bool IsLive { get; }

        /// <summary>
        /// Asks the wallet provider to send a one-time code to the wallet's
        /// phone. Returns the message to show the customer.
        /// </summary>
        string SendWalletCode(string method, string walletNumber);

        /// <summary>Returns the authorization, or null with a reason the customer can act on.</summary>
        PaymentAuthorization AuthorizeWallet(string method, string walletNumber, string code, string pin,
                                             decimal amount, out string failureReason);

        /// <summary>Returns the authorization, or null with a reason the customer can act on.</summary>
        PaymentAuthorization AuthorizeCard(string cardNumber, string expiry, string cvv, string cardHolder,
                                           decimal amount, out string failureReason);
    }

    public static class PaymentGateways
    {
        /// <summary>The gateway the checkout uses.</summary>
        public static IPaymentGateway Current { get; } = new SandboxPaymentGateway();
    }

    /// <summary>
    /// A test gateway that behaves like the real ones without any money moving.
    ///
    ///  - bKash / Nagad: the "SMS" code is generated here and shown on the
    ///    payment screen, because no SMS is sent. It expires after three
    ///    minutes and allows three wrong tries. Any PIN of the right length
    ///    (five digits for bKash, four for Nagad) is accepted, except all
    ///    zeros, which stands for a wrong PIN.
    ///  - Visa: any valid Visa number with a future expiry and a three digit
    ///    CVV is approved, except the standard test number
    ///    4000 0000 0000 0002, which is always declined.
    /// </summary>
    public class SandboxPaymentGateway : IPaymentGateway
    {
        public const string DeclinedTestCard = "4000000000000002";

        private const int CodeLifetimeMinutes = 3;
        private const int MaxCodeTries = 3;

        private sealed class PendingCode
        {
            public string Code = "";
            public DateTime ExpiresAt;
            public int WrongTries;
        }

        /// <summary>One live code per method and wallet, e.g. "bKash:01811000006".</summary>
        private readonly Dictionary<string, PendingCode> _codes = new Dictionary<string, PendingCode>();

        public bool IsLive => false;

        public static int PinLength(string method) => method == "bKash" ? 5 : 4;

        public string SendWalletCode(string method, string walletNumber)
        {
            string code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            _codes[method + ":" + walletNumber] = new PendingCode
            {
                Code = code,
                ExpiresAt = DateTime.Now.AddMinutes(CodeLifetimeMinutes)
            };

            return "Sandbox: no SMS is sent. The " + method + " verification code for " + walletNumber +
                   " is " + code + " (valid for " + CodeLifetimeMinutes + " minutes).";
        }

        public PaymentAuthorization AuthorizeWallet(string method, string walletNumber, string code, string pin,
                                                    decimal amount, out string failureReason)
        {
            failureReason = "";

            if (method != "bKash" && method != "Nagad")
            {
                failureReason = "This payment method is not a mobile wallet.";
                return null;
            }
            if (!Validator.IsMobile(walletNumber))
            {
                failureReason = "Enter the 11 digit " + method + " account number.";
                return null;
            }

            PendingCode pending;
            string key = method + ":" + walletNumber;
            if (!_codes.TryGetValue(key, out pending))
            {
                failureReason = "Press 'Send code' first to get a verification code.";
                return null;
            }
            if (DateTime.Now > pending.ExpiresAt)
            {
                _codes.Remove(key);
                failureReason = "The verification code has expired. Press 'Send code' for a new one.";
                return null;
            }
            if ((code ?? "").Trim() != pending.Code)
            {
                pending.WrongTries++;
                if (pending.WrongTries >= MaxCodeTries)
                {
                    _codes.Remove(key);
                    failureReason = "The verification code was wrong three times. Press 'Send code' for a new one.";
                }
                else
                {
                    failureReason = "That verification code is not correct.";
                }
                return null;
            }

            int pinLength = PinLength(method);
            if (!Validator.IsWalletPin(pin, pinLength) || pin == new string('0', pinLength))
            {
                failureReason = "The " + method + " PIN is not correct.";
                return null;
            }

            // A code is used once, whether or not anything after it succeeds.
            _codes.Remove(key);

            return new PaymentAuthorization
            {
                Method = method,
                WalletNumber = walletNumber,
                TransactionRef = NewReference(method == "bKash" ? "BK" : "NG"),
                Amount = amount,
                AuthorizedAt = DateTime.Now
            };
        }

        public PaymentAuthorization AuthorizeCard(string cardNumber, string expiry, string cvv, string cardHolder,
                                                  decimal amount, out string failureReason)
        {
            failureReason = "";
            string digits = Validator.CardDigits(cardNumber);

            if (!Validator.IsVisaCardNumber(digits))
            {
                failureReason = "This is not a valid Visa card number.";
                return null;
            }
            if (!Validator.IsCardExpiry(expiry))
            {
                failureReason = "The card has expired, or the expiry date is not written MM/YY.";
                return null;
            }
            if (!Validator.IsCvv(cvv))
            {
                failureReason = "Enter the three digit security code from the back of the card.";
                return null;
            }
            if (Validator.IsBlank(cardHolder))
            {
                failureReason = "Enter the name printed on the card.";
                return null;
            }
            if (digits == DeclinedTestCard)
            {
                failureReason = "The card issuer declined this payment. Try another card or another payment method.";
                return null;
            }

            return new PaymentAuthorization
            {
                Method = "Card",
                CardLast4 = digits.Substring(digits.Length - 4),
                TransactionRef = NewReference("VS"),
                Amount = amount,
                AuthorizedAt = DateTime.Now
            };
        }

        /// <summary>
        /// A reference such as BK261029X7K2QP: method prefix, date, and six
        /// random characters. UX_Payments_TransactionRef keeps it unique.
        /// </summary>
        private static string NewReference(string prefix)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] tail = new char[6];
            for (int i = 0; i < tail.Length; i++)
                tail[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            return prefix + DateTime.Now.ToString("yyMMdd") + new string(tail);
        }
    }
}

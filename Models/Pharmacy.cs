namespace PharmaLinkApp.Models
{
    /// <summary>
    /// One pharmacy. OwnerId is UNIQUE in the database, which is what enforces
    /// the rule that one pharmacy owner owns exactly one pharmacy.
    /// </summary>
    public class Pharmacy
    {
        public int PharmacyId { get; set; }
        public int OwnerId { get; set; }
        public string PharmacyName { get; set; } = "";
        public string LicenseNo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Address { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public decimal CommissionRate { get; set; }
        public string Status { get; set; } = "";        // Pending | Approved | Suspended
        public DateTime RegisteredAt { get; set; }

        public string OwnerName { get; set; } = "";

        // -- the Super Admin's warning -------------------------------------------
        // A pharmacy holds at most one current warning. Sending a new one
        // overwrites the message, restamps WarnedAt and clears the
        // acknowledgement, so the owner always sees the latest notice once.

        /// <summary>The text of the current warning, or empty when the shop has never been warned.</summary>
        public string WarningMessage { get; set; } = "";

        /// <summary>When the Super Admin sent the current warning; null when there is none.</summary>
        public DateTime? WarnedAt { get; set; }

        /// <summary>When the owner pressed "I've read this"; null while the warning is still unread.</summary>
        public DateTime? WarningAcknowledgedAt { get; set; }

        /// <summary>True when there is a warning the owner has not acknowledged yet, i.e. the dashboard banner should show.</summary>
        public bool HasUnreadWarning =>
            WarnedAt.HasValue && !WarningAcknowledgedAt.HasValue && !string.IsNullOrWhiteSpace(WarningMessage);

        public override string ToString() => PharmacyName;
    }
}

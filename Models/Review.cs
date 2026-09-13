namespace PharmaLinkApp.Models
{
    /// <summary>
    /// A rating and comment. OrderId is carried so the application can prove the
    /// reviewer actually bought the item, and IsHidden lets the Super Admin
    /// moderate abuse without destroying the audit trail.
    /// </summary>
    public class Review
    {
        public int ReviewId { get; set; }
        public int CustomerId { get; set; }
        public int MedicineId { get; set; }
        public int OrderId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime ReviewDate { get; set; }
        public bool IsHidden { get; set; }

        /// <summary>Set when the pharmacy owner flags the review for the Super Admin to look at.</summary>
        public bool IsReported { get; set; }
        public string ReportReason { get; set; } = "";
        public DateTime? ReportedAt { get; set; }

        public string ReviewerName { get; set; } = "";
        public string MedicineName { get; set; } = "";
        public string PharmacyName { get; set; } = "";
    }
}

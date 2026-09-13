namespace PharmaLinkApp.Models
{
    /// <summary>
    /// The photograph of a doctor's prescription attached to an order that
    /// contains a medicine whose RequiresRx flag is set. The order cannot be
    /// confirmed while VerifyStatus is still Pending.
    /// </summary>
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string ImagePath { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public DateTime UploadedAt { get; set; }
        public string VerifyStatus { get; set; } = "";   // Pending | Approved | Rejected

        public string CustomerName { get; set; } = "";
    }
}

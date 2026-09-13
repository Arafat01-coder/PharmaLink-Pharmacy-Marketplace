namespace PharmaLinkApp.Models
{
    /// <summary>
    /// A percentage discount on one medicine, valid between two dates.
    /// The discounted price itself is always computed inside the SQL query, so
    /// this class only carries what the grid needs to display.
    /// </summary>
    public class Offer
    {
        public int OfferId { get; set; }
        public int MedicineId { get; set; }
        public string OfferTitle { get; set; } = "";
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public string MedicineName { get; set; } = "";
        public string PharmacyName { get; set; } = "";
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
    }
}

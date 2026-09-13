namespace PharmaLinkApp.Models
{
    /// <summary>
    /// A product on a pharmacy's shelf. Two pharmacies selling the same brand
    /// are two separate rows, each with its own price and its own stock.
    /// </summary>
    public class Medicine
    {
        public int MedicineId { get; set; }
        public int PharmacyId { get; set; }
        public int CategoryId { get; set; }
        public string MedicineName { get; set; } = "";
        public string GenericName { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Strength { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public bool RequiresRx { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Description { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public bool IsActive { get; set; } = true;

        // filled by joins when a screen needs them
        public string CategoryName { get; set; } = "";
        public string PharmacyName { get; set; } = "";
        public string Area { get; set; } = "";
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// True only when a customer may add this medicine to a cart right now:
        /// it is listed, its pharmacy is Approved and it has not expired.
        /// Filled by MedicineService.GetDetails.
        /// </summary>
        public bool CanBuy { get; set; }

        public decimal PriceAfterDiscount =>
            decimal.Round(UnitPrice * (1 - DiscountPercent / 100m), 2);

        public bool IsLowStock => Stock < MinStock;
    }
}

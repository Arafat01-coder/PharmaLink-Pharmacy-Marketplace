namespace PharmaLinkApp.Models
{
    /// <summary>
    /// One line of the customer's live basket, already carrying whatever
    /// discount is running today so the cart and the checkout can never
    /// disagree about the price.
    /// </summary>
    public class CartLine
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }

        public string MedicineName { get; set; } = "";
        public string Strength { get; set; } = "";
        public int PharmacyId { get; set; }
        public string PharmacyName { get; set; } = "";
        public decimal ListPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal PriceYouPay { get; set; }
        public int Stock { get; set; }
        public bool RequiresRx { get; set; }

        public decimal LineTotal => decimal.Round(PriceYouPay * Quantity, 2);
    }
}

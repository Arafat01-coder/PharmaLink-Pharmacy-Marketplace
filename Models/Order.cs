namespace PharmaLinkApp.Models
{
    /// <summary>
    /// One completed checkout, for one pharmacy. A basket that spans two
    /// pharmacies becomes two of these.
    /// CommissionAmount is frozen at checkout time and never recalculated.
    /// </summary>
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int PharmacyId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal ItemsTotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        /// <summary>The bKash or Nagad wallet number the customer paid from; empty for cash and card.</summary>
        public string PaymentMobile { get; set; } = "";
        public string Status { get; set; } = "";

        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string PharmacyName { get; set; } = "";
        public string PharmacyAddress { get; set; } = "";
        public string PharmacyLicense { get; set; } = "";
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

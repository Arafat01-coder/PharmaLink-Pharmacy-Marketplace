namespace PharmaLinkApp.Models
{
    /// <summary>
    /// A row of the junction table that resolves the many to many relationship
    /// between Orders and Medicines. UnitPrice is the price on the day of the
    /// purchase, not today's price.
    /// </summary>
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public string MedicineName { get; set; } = "";
        public string Strength { get; set; } = "";
    }
}

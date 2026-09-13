namespace PharmaLinkApp.Models
{
    /// <summary>
    /// A medicine category from the master list the Super Admin maintains.
    /// A referenced category is deactivated rather than deleted, so the foreign
    /// keys pointing at it from Medicines stay valid.
    /// </summary>
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = true;

        /// <summary>ComboBox controls display this.</summary>
        public override string ToString() => CategoryName;
    }
}

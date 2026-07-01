namespace GymManagement.Models
{
    public class ProductMaster
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int? VendorId { get; set; }
        public bool IsActive { get; set; } = true;

        public string? VendorName { get; set; }
    }
}

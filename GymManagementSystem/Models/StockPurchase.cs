namespace GymManagement.Models
{
    public class StockPurchase
    {
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public int VendorId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? InvoiceNo { get; set; }
        public string? Remarks { get; set; }

        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? VendorName { get; set; }
        public int? CurrentStock { get; set; }
    }
}
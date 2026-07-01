namespace GymManagement.Models
{
    public class StockIssue
    {
        public int IssueId { get; set; }
        public int ProductId { get; set; }
        public int? MemberId { get; set; }
        public int Quantity { get; set; }
        public DateTime IssueDate { get; set; }
        public string IssuedTo { get; set; } = "";
        public decimal? Amount { get; set; }
        public string? PaymentMode { get; set; }
        public string? Remarks { get; set; }

        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
        public int? CurrentStock { get; set; }
    }
}

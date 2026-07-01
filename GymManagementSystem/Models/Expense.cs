namespace GymManagement.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int ExpenseHeadId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public string? PaidTo { get; set; }
        public string? Remarks { get; set; }

        public string? HeadName { get; set; }
    }
}

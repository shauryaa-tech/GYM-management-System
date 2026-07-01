namespace GymManagement.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int MemberId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }

        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
    }
}

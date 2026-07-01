namespace GymManagement.Models
{
    public class MembershipTransaction
    {
        public int TransactionId { get; set; }

        public int MemberId { get; set; }

        public int PlanId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Amount { get; set; }

        public string? PaymentStatus { get; set; }

        public string? MembershipStatus { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; }

        // Display Purpose
        public string? MemberName { get; set; }

        public string? PlanName { get; set; }
    }
}
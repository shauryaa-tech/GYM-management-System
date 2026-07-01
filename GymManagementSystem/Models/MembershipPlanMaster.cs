namespace GymManagement.Models
{
    public class MembershipPlanMaster
    {
        public int PlanId { get; set; }

        public string PlanName { get; set; }

        public int DurationMonths { get; set; }

        public decimal Amount { get; set; }

        public decimal JoiningFee { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
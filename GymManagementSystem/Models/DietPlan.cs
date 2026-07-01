namespace GymManagement.Models
{
    public class DietPlan
    {
        public int DietPlanId { get; set; }
        public int MemberId { get; set; }
        public string PlanName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? CalorieTarget { get; set; }
        public string? Remarks { get; set; }

        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
    }
}

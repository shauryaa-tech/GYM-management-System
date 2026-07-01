namespace GymManagement.Models
{
    public class WorkoutPlan
    {
        public int PlanId { get; set; }
        public int MemberId { get; set; }
        public int? TrainerId { get; set; }
        public string PlanName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Goals { get; set; }
        public string? Remarks { get; set; }

        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
        public string? TrainerName { get; set; }
    }
}

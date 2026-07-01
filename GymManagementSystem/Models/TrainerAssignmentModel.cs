namespace GymManagement.Models
{
    public class TrainerAssignmentModel
    {
        public int AssignmentId { get; set; }

        public int MemberId { get; set; }

        public string MemberName { get; set; } = "";

        public int TrainerId { get; set; }

        public string TrainerName { get; set; } = "";

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Remarks { get; set; } = "";

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }
    }
}
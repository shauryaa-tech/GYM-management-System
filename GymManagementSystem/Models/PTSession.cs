namespace GymManagement.Models
{
    public class PTSession
    {
        public int SessionId { get; set; }
        public int MemberId { get; set; }
        public int TrainerId { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
        public string? TrainerName { get; set; }
    }
}

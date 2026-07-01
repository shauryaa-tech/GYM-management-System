namespace GymManagement.Models
{
    public class ClassMaster
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = "";
        public int? TrainerId { get; set; }
        public string? Schedule { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? MaxCapacity { get; set; }
        public decimal? Amount { get; set; }
        public bool IsActive { get; set; } = true;

        public string? TrainerName { get; set; }
    }
}

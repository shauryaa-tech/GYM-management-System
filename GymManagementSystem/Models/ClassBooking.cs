namespace GymManagement.Models
{
    public class ClassBooking
    {
        public int BookingId { get; set; }
        public int MemberId { get; set; }
        public int ClassId { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = "Confirmed";
        public decimal? Amount { get; set; }
        public string? Remarks { get; set; }

        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
        public string? ClassName { get; set; }
        public string? TrainerName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
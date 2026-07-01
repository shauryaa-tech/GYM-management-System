namespace GymManagement.Models
{
    public class SalaryRuleMaster
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int WorkingDaysPerMonth { get; set; } = 26;
        public bool AbsentDeductionPerDay { get; set; } = true;
        public decimal HalfDayDeductionFactor { get; set; } = 0.5m;
        public bool LeaveIsPaid { get; set; } = true;
        public int LateGraceMinutes { get; set; } = 15;
        public TimeSpan? ShiftStartTime { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? ShiftEndTime { get; set; } = new TimeSpan(18, 0, 0);
        public int EarlyLeaveGraceMinutes { get; set; }
        public bool EnableSandwichRule { get; set; }
        public string? WeeklyOffDays { get; set; } = "Sunday";
        public bool LateCountsAsHalfDay { get; set; } = true;
        public bool EarlyLeaveCountsAsHalfDay { get; set; } = true;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ShiftDisplay =>
            ShiftStartTime.HasValue && ShiftEndTime.HasValue
                ? $"{DateTime.Today.Add(ShiftStartTime.Value):hh:mm tt} – {DateTime.Today.Add(ShiftEndTime.Value):hh:mm tt}"
                : "—";
    }
}

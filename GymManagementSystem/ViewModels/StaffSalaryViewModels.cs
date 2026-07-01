namespace GymManagement.ViewModels
{
    public class StaffTodayAttendanceRow
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public int? AttendanceId { get; set; }
        public string DayStatus { get; set; } = "NotMarked";
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Remarks { get; set; }

        public string? FormattedCheckIn => GymManagement.Helpers.TimeFormatHelper.FormatTime12(CheckInTime);
        public string? FormattedCheckOut => GymManagement.Helpers.TimeFormatHelper.FormatTime12(CheckOutTime);

        public string StatusBadgeClass => DayStatus switch
        {
            "Present" => "bg-success",
            "Absent" => "bg-danger",
            "Leave" => "bg-warning text-dark",
            "HalfDay" => "bg-info",
            _ => "bg-secondary"
        };

        public string StatusLabel => DayStatus switch
        {
            "Present" => "Present",
            "Absent" => "Absent",
            "Leave" => "On Leave",
            "HalfDay" => "Half Day",
            _ => "Not Marked"
        };

        public string? WorkDuration
        {
            get
            {
                if (CheckInTime == null || CheckOutTime == null) return null;
                var span = CheckOutTime.Value - CheckInTime.Value;
                if (span.TotalMinutes < 0) return null;
                return $"{(int)span.TotalHours}h {span.Minutes}m";
            }
        }
    }

    public class StaffAttendanceSummary
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public decimal BasicSalary { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public int TotalMarkedDays { get; set; }
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int WorkingDaysPerMonth { get; set; }
        public decimal PerDaySalary { get; set; }
        public decimal DeductionDays { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public bool AlreadyProcessed { get; set; }
        public bool HasSalaryRecord { get; set; }
        public bool IsPaid { get; set; }
        public bool IsPaymentPending { get; set; }
        public int? ExistingSalaryId { get; set; }
        public string? ExistingPaymentMode { get; set; }
        public DateTime? ExistingPaidDate { get; set; }
        public string RuleSummary { get; set; } = string.Empty;
        public decimal LatePenaltyDays { get; set; }
        public decimal EarlyLeavePenaltyDays { get; set; }
        public int SandwichAbsentDays { get; set; }
        public List<StaffAttendanceDayRow> DailyRows { get; set; } = new();
    }

    public class StaffAttendanceDayRow
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Absent";
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
    }

    public class StaffMonthlySummaryRow
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public decimal BasicSalary { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public int TotalMarked { get; set; }
        public bool SalaryProcessed { get; set; }
    }
}

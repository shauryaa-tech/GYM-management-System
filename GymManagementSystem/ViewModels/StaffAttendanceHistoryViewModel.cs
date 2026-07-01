namespace GymManagement.ViewModels
{
    public class StaffAttendanceHistoryViewModel
    {
        public int? SelectedStaffId { get; set; }
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public List<StaffAttendanceStaffOption> StaffList { get; set; } = new();
        public StaffAttendanceStaffOption? SelectedStaff { get; set; }
        public StaffMonthAttendanceBlock SelectedMonthBlock { get; set; } = new();
    }

    public class StaffAttendanceStaffOption
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = "";
        public string? Designation { get; set; }
        public string? MobileNo { get; set; }
        public decimal BasicSalary { get; set; }
        public string Display => string.IsNullOrWhiteSpace(Designation)
            ? StaffName
            : $"{StaffName} — {Designation}";
    }

    public class StaffMonthAttendanceBlock
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel { get; set; } = "";
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public int TotalMarked { get; set; }
        public string TotalWorkTime { get; set; } = "0h 0m";
        public List<StaffHistoryDayRow> Days { get; set; } = new();
    }

    public class StaffHistoryDayRow
    {
        public int AttendanceId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = "Absent";
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Remarks { get; set; }

        public string? FormattedCheckIn => Helpers.TimeFormatHelper.FormatTime12(CheckInTime);
        public string? FormattedCheckOut => Helpers.TimeFormatHelper.FormatTime12(CheckOutTime);

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

        public string StatusBadgeClass => Status switch
        {
            "Present" => "bg-success",
            "Absent" => "bg-danger",
            "Leave" => "bg-warning text-dark",
            "HalfDay" => "bg-info",
            _ => "bg-secondary"
        };

        public string StatusLabel => Status switch
        {
            "Present" => "Present",
            "Absent" => "Absent",
            "Leave" => "On Leave",
            "HalfDay" => "Half Day",
            _ => Status
        };
    }
}

namespace GymManagement.ViewModels
{
    public class MemberAttendanceHistoryViewModel
    {
        public int? SelectedMemberId { get; set; }
        public List<MemberAttendanceMemberOption> Members { get; set; } = new();
        public MemberAttendanceMemberOption? SelectedMember { get; set; }
        public MemberMonthAttendanceBlock CurrentMonth { get; set; } = new();
        public MemberMonthAttendanceBlock PreviousMonth { get; set; } = new();
    }

    public class MemberAttendanceMemberOption
    {
        public int MemberId { get; set; }
        public string MemberCode { get; set; } = "";
        public string MemberName { get; set; } = "";
        public string? MobileNo { get; set; }
        public string? PlanName { get; set; }
        public string Display => $"{MemberCode} - {MemberName}";
    }

    public class MemberMonthAttendanceBlock
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel { get; set; } = "";
        public int TotalVisitDays { get; set; }
        public int CompletedSessions { get; set; }
        public string TotalGymTime { get; set; } = "0h 0m";
        public List<MemberAttendanceDayRow> Days { get; set; } = new();
    }

    public class MemberAttendanceDayRow
    {
        public int AttendanceId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Remarks { get; set; }

        public string? FormattedCheckIn => Helpers.TimeFormatHelper.FormatTime12(CheckInTime);
        public string? FormattedCheckOut => Helpers.TimeFormatHelper.FormatTime12(CheckOutTime);

        public string? Duration
        {
            get
            {
                if (CheckInTime == null || CheckOutTime == null) return null;
                var span = CheckOutTime.Value - CheckInTime.Value;
                if (span.TotalMinutes < 0) return null;
                return $"{(int)span.TotalHours}h {span.Minutes}m";
            }
        }

        public string StatusLabel => CheckInTime != null ? "Present" : "Marked";
    }
}

namespace GymManagement.ViewModels
{
    public class DashboardViewModel
    {
        public string? FullName { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public bool IsSelectedToday { get; set; } = true;

        // KPIs
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int TodayAttendance { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int OutstandingCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int ExpiredCount { get; set; }
        public int PendingLeads { get; set; }
        public int NewLeadsToday { get; set; }
        public int FollowUpDue { get; set; }
        public int NewMembersThisMonth { get; set; }

        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int StaffAttendanceToday { get; set; }

        // Charts
        public List<string> Months { get; set; } = new();
        public List<decimal> Revenues { get; set; } = new();
        public List<string> AttendanceDates { get; set; } = new();
        public List<int> MemberAttendance { get; set; } = new();
        public List<int> TrainerAttendance { get; set; } = new();
        public List<string> PaymentModes { get; set; } = new();
        public List<decimal> PaymentModeAmounts { get; set; } = new();

        // Tables
        public List<DashboardMemberRow> RecentMembers { get; set; } = new();
        public List<DashboardExpiryRow> ExpiringMembers { get; set; } = new();
        public List<DashboardOutstandingRow> OutstandingRows { get; set; } = new();
        public List<DashboardLeadRow> RecentLeads { get; set; } = new();
        public List<DashboardLeadRow> FollowUpLeads { get; set; } = new();
        public List<StaffTodayAttendanceRow> StaffTodayBoard { get; set; } = new();
        public bool ShowStaffBoard { get; set; }
    }

    public class DashboardMemberRow
    {
        public string MemberName { get; set; } = string.Empty;
        public string? MemberCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }
    }

    public class DashboardExpiryRow
    {
        public string MemberName { get; set; } = string.Empty;
        public string? PlanName { get; set; }
        public DateTime? PlanEndDate { get; set; }
        public int DaysLeft { get; set; }
    }

    public class DashboardOutstandingRow
    {
        public string MemberName { get; set; } = string.Empty;
        public string? PlanName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class DashboardLeadRow
    {
        public int LeadId { get; set; }
        public string LeadName { get; set; } = string.Empty;
        public string? MobileNo { get; set; }
        public string? Status { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

namespace GymManagement.ViewModels
{
    public class AttendanceReportRow
    {
        public int AttendanceId { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public List<AttendanceReportRow> Rows { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalRecords { get; set; }
        public int UniqueMembers { get; set; }
    }

    public class ExpiryReportRow
    {
        public int MemberId { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public string? MobileNo { get; set; }
        public string? PlanName { get; set; }
        public DateTime PlanEndDate { get; set; }
        public int DaysLeft { get; set; }
        public string? Status { get; set; }
    }

    public class ExpiryReportViewModel
    {
        public List<ExpiryReportRow> Rows { get; set; } = new();
        public int DaysAhead { get; set; }
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
    }

    public class CollectionReportRow
    {
        public int PaymentId { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNo { get; set; }
    }

    public class CollectionReportViewModel
    {
        public List<CollectionReportRow> Rows { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? PaymentMode { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalRecords { get; set; }
        public decimal CashTotal { get; set; }
        public decimal UpiTotal { get; set; }
        public decimal CardTotal { get; set; }
    }

    public class OutstandingReportRow
    {
        public int TransactionId { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public string? MobileNo { get; set; }
        public string? PlanName { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class OutstandingReportViewModel
    {
        public List<OutstandingReportRow> Rows { get; set; } = new();
        public string? StatusFilter { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int PendingCount { get; set; }
        public int PartialCount { get; set; }
    }

    public class ProfitLossReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSalaries { get; set; }
        public decimal NetProfit { get; set; }
        public List<ProfitLossBreakdownRow> ExpenseBreakdown { get; set; } = new();
        public List<ProfitLossBreakdownRow> IncomeByMode { get; set; } = new();
    }

    public class ProfitLossBreakdownRow
    {
        public string Label { get; set; } = "";
        public decimal Amount { get; set; }
    }
}

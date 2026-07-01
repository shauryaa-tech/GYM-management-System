namespace GymManagement.ViewModels
{
    public class PayrollDashboardViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel { get; set; } = "";
        public int ActiveStaff { get; set; }
        public int ProcessedCount { get; set; }
        public int PendingCount => Math.Max(0, ActiveStaff - ProcessedCount);
        public decimal TotalNetPaid { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalBasic { get; set; }
    }
}

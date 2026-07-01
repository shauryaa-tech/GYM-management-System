namespace GymManagement.ViewModels
{
    public class BankStatementReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel { get; set; } = "";
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public string CompanyName { get; set; } = "";
        public string? DepartmentFilter { get; set; }
        public string? Search { get; set; }
        public bool OnlyWithBank { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<BankStatementRowViewModel> Rows { get; set; } = new();

        public decimal TotalAmount => Rows.Sum(r => r.Amount);
    }

    public class BankStatementRowViewModel
    {
        public string EmpCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? IfscCode { get; set; }
        public decimal Amount { get; set; }
        public string? Department { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsPaid { get; set; }
    }
}

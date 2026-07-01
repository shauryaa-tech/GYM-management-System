namespace GymManagement.ViewModels
{
    public class SalaryStatementReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string CompanyAddress { get; set; } = "";
        public string RuleName { get; set; } = "";
        public int WorkingDaysPerMonth { get; set; }
        public List<SalaryStatementRowViewModel> Rows { get; set; } = new();

        public decimal TotalGross => Rows.Sum(r => r.Gross);
        public decimal TotalDeductions => Rows.Sum(r => r.Deductions);
        public decimal TotalNet => Rows.Sum(r => r.NetPay);
    }

    public class SalaryStatementRowViewModel
    {
        public int StaffId { get; set; }
        public string EmpCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Company { get; set; } = "";
        public string Department { get; set; } = "";
        public string Designation { get; set; } = "";

        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int WeeklyOffDays { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public string OtHours { get; set; } = "00:00";
        public decimal TotalDays { get; set; }

        public decimal Basic { get; set; }
        public decimal Gross { get; set; }
        public decimal EarnedGross { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal PerDaySalary { get; set; }
        public decimal DeductionDays { get; set; }

        public bool HasSalaryRecord { get; set; }
        public bool IsPaid { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? PaymentMode { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? RuleName { get; set; }

        public bool IsProcessed => HasSalaryRecord && IsPaid;
    }
}

namespace GymManagement.Models
{
    public class SalaryProcessing
    {
        public int SalaryId { get; set; }
        public int StaffId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PaymentMode { get; set; }
        public string? Remarks { get; set; }
        public int? PresentDays { get; set; }
        public int? AbsentDays { get; set; }
        public int? LeaveDays { get; set; }
        public int? HalfDays { get; set; }
        public int? SalaryRuleId { get; set; }

        public string? StaffName { get; set; }
        public string? StaffCode { get; set; }
        public string? Designation { get; set; }

        public string DisplayStaffCode =>
            string.IsNullOrWhiteSpace(StaffCode) ? $"EMP{StaffId:D3}" : StaffCode!;

        public bool IsPaid =>
            PaidDate.HasValue &&
            !string.Equals(PaymentMode, "Pending", StringComparison.OrdinalIgnoreCase);

        public bool IsPaymentPending => SalaryId > 0 && !IsPaid;

        public string PaymentStatus => IsPaid ? "Paid" : IsPaymentPending ? "Pending" : "";
    }
}

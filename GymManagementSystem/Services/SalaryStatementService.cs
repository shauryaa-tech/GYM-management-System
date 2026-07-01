using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.ViewModels;

namespace GymManagement.Services
{
    public class SalaryStatementService
    {
        private readonly StaffRepository _staffRepo;
        private readonly SalaryRuleMasterRepository _ruleRepo;
        private readonly SalaryCalculationService _calcService;
        private readonly SalaryProcessingRepository _salaryRepo;
        private readonly IConfiguration _config;

        public SalaryStatementService(
            StaffRepository staffRepo,
            SalaryRuleMasterRepository ruleRepo,
            SalaryCalculationService calcService,
            SalaryProcessingRepository salaryRepo,
            IConfiguration config)
        {
            _staffRepo = staffRepo;
            _ruleRepo = ruleRepo;
            _calcService = calcService;
            _salaryRepo = salaryRepo;
            _config = config;
        }

        public SalaryStatementReportViewModel Build(int month, int year, int? ruleId = null)
        {
            var rule = _ruleRepo.ResolveRule(ruleId)
                ?? throw new InvalidOperationException("No active salary rule found. Add a rule in Salary Rule Master.");

            var companyName = (_config["Gym:Name"] ?? "").Trim();
            var companyAddress = (_config["Gym:Address"] ?? "").Trim();

            var processed = _salaryRepo
                .GetAll(null, null, month.ToString(), year.ToString())
                .GroupBy(s => s.StaffId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SalaryId).First());

            var report = new SalaryStatementReportViewModel
            {
                Month = month,
                Year = year,
                MonthLabel = new DateTime(year, month, 1).ToString("MMM-yyyy"),
                CompanyName = companyName,
                CompanyAddress = companyAddress,
                RuleName = rule.RuleName,
                WorkingDaysPerMonth = rule.WorkingDaysPerMonth > 0 ? rule.WorkingDaysPerMonth : 26
            };

            foreach (var staff in _staffRepo.GetAll().Where(s => s.IsActive).OrderBy(s => s.StaffName))
            {
                processed.TryGetValue(staff.StaffId, out var saved);
                var summary = _calcService.Calculate(staff.StaffId, month, year, rule.RuleId);

                var basic = saved?.BasicSalary ?? summary.BasicSalary;
                var deductions = saved?.Deductions ?? summary.Deductions;
                var netPay = saved?.NetSalary ?? summary.NetSalary;
                var presentDays = saved?.PresentDays ?? summary.PresentDays;
                var absentDays = saved?.AbsentDays ?? summary.AbsentDays;
                var leaveDays = saved?.LeaveDays ?? summary.LeaveDays;
                var halfDays = saved?.HalfDays ?? summary.HalfDays;
                var perDay = summary.WorkingDaysPerMonth > 0
                    ? basic / summary.WorkingDaysPerMonth
                    : summary.PerDaySalary;
                var deductionDays = saved != null && saved.Deductions > 0 && perDay > 0
                    ? Math.Round(saved.Deductions / perDay, 2)
                    : summary.DeductionDays;

                var wo = SalaryRuleEngine.CountWeeklyOffDaysInMonth(rule.WeeklyOffDays, month, year);
                var totalDays = SalaryRuleEngine.CalculatePayableDays(
                    presentDays, leaveDays, halfDays, rule);
                var otHours = SalaryRuleEngine.FormatOtHours(summary.DailyRows, rule);
                var earnedGross = Math.Max(0, basic - deductions);
                var department = ResolveDepartment(staff);

                report.Rows.Add(new SalaryStatementRowViewModel
                {
                    StaffId = staff.StaffId,
                    EmpCode = staff.DisplayStaffCode,
                    EmployeeName = staff.StaffName,
                    Company = companyName,
                    Department = department,
                    Designation = staff.Designation,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    WeeklyOffDays = wo,
                    LeaveDays = leaveDays,
                    HalfDays = halfDays,
                    OtHours = otHours,
                    TotalDays = totalDays,
                    Basic = basic,
                    Gross = basic,
                    EarnedGross = earnedGross,
                    Deductions = deductions,
                    NetPay = netPay,
                    PerDaySalary = perDay,
                    DeductionDays = deductionDays,
                    HasSalaryRecord = saved != null,
                    IsPaid = saved?.IsPaid ?? false,
                    PaymentStatus = saved == null ? "Pending" : saved.PaymentStatus,
                    PaymentMode = saved?.PaymentMode,
                    PaidDate = saved?.PaidDate,
                    RuleName = summary.RuleName
                });
            }

            return report;
        }

        private static string ResolveDepartment(StaffMaster staff)
        {
            if (!string.IsNullOrWhiteSpace(staff.RoleName))
                return staff.RoleName!;
            if (!string.IsNullOrWhiteSpace(staff.Designation))
                return staff.Designation;
            return "";
        }
    }
}

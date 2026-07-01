using GymManagement.Data.Repositories;
using GymManagement.ViewModels;

namespace GymManagement.Services
{
    public class BankStatementService
    {
        private readonly SalaryStatementService _salaryStatementService;
        private readonly StaffRepository _staffRepo;
        private readonly IConfiguration _config;

        public BankStatementService(
            SalaryStatementService salaryStatementService,
            StaffRepository staffRepo,
            IConfiguration config)
        {
            _salaryStatementService = salaryStatementService;
            _staffRepo = staffRepo;
            _config = config;
        }

        public BankStatementReportViewModel Build(
            int month,
            int year,
            DateTime? paymentDate = null,
            string? department = null,
            string? search = null,
            bool onlyWithBank = false,
            int? ruleId = null)
        {
            var salaryReport = _salaryStatementService.Build(month, year, ruleId);
            var staffMap = _staffRepo.GetAll().ToDictionary(s => s.StaffId);

            var departments = staffMap.Values
                .Select(s => string.IsNullOrWhiteSpace(s.RoleName) ? "General" : s.RoleName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var report = new BankStatementReportViewModel
            {
                Month = month,
                Year = year,
                MonthLabel = salaryReport.MonthLabel,
                PaymentDate = paymentDate ?? DateTime.Today,
                CompanyName = salaryReport.CompanyName,
                DepartmentFilter = department,
                Search = search,
                OnlyWithBank = onlyWithBank,
                Departments = departments
            };

            foreach (var row in salaryReport.Rows)
            {
                if (!staffMap.TryGetValue(row.StaffId, out var staff))
                    continue;

                var dept = string.IsNullOrWhiteSpace(staff.RoleName) ? "General" : staff.RoleName!;
                if (!string.IsNullOrWhiteSpace(department) &&
                    !dept.Equals(department, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (onlyWithBank &&
                    string.IsNullOrWhiteSpace(staff.BankAccountNo) &&
                    string.IsNullOrWhiteSpace(staff.BankName))
                    continue;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.Trim();
                    if (!row.EmployeeName.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                        !staff.DisplayStaffCode.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                        !(staff.BankAccountNo?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
                        continue;
                }

                if (row.NetPay <= 0)
                    continue;

                report.Rows.Add(new BankStatementRowViewModel
                {
                    EmpCode = staff.DisplayStaffCode,
                    EmployeeName = row.EmployeeName,
                    CompanyName = salaryReport.CompanyName,
                    BankName = staff.BankName,
                    BankAccountNo = staff.BankAccountNo,
                    IfscCode = staff.IfscCode,
                    Amount = row.NetPay,
                    Department = dept,
                    IsProcessed = row.HasSalaryRecord,
                    IsPaid = row.IsPaid
                });
            }

            return report;
        }
    }
}

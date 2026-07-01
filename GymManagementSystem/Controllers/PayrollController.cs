using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class PayrollController : Controller
    {
        private readonly SalaryProcessingRepository _salaryRepo;
        private readonly StaffRepository _staffRepo;

        public PayrollController(
            SalaryProcessingRepository salaryRepo,
            StaffRepository staffRepo)
        {
            _salaryRepo = salaryRepo;
            _staffRepo = staffRepo;
        }

        [Permission("Payroll", "View")]
        public IActionResult Index(int? month, int? year)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            var activeStaff = _staffRepo.GetAll().Count(s => s.IsActive);
            var salaries = _salaryRepo.GetAll(null, null, m.ToString(), y.ToString());

            var model = new PayrollDashboardViewModel
            {
                Month = m,
                Year = y,
                MonthLabel = new DateTime(y, m, 1).ToString("MMMM yyyy"),
                ActiveStaff = activeStaff,
                ProcessedCount = salaries.Count,
                TotalNetPaid = salaries.Sum(s => s.NetSalary),
                TotalDeductions = salaries.Sum(s => s.Deductions),
                TotalBasic = salaries.Sum(s => s.BasicSalary)
            };

            return View(model);
        }
    }
}

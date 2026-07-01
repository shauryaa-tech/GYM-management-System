using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportRepository _repo;

        public ReportsController(ReportRepository repo)
        {
            _repo = repo;
        }

        [Permission("ReportAttendance", "View")]
        public IActionResult Attendance(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            if (from > to)
            {
                (from, to) = (to, from);
            }

            var model = _repo.GetAttendanceReport(from, to);
            return View(model);
        }

        [Permission("ReportExpiry", "View")]
        public IActionResult MembershipExpiry(int? daysAhead)
        {
            int days = daysAhead ?? 30;
            if (days < 1) days = 1;
            if (days > 365) days = 365;

            var model = _repo.GetExpiryReport(days);
            return View(model);
        }

        [Permission("ReportCollections", "View")]
        public IActionResult Collections(DateTime? fromDate, DateTime? toDate, string? mode)
        {
            var from = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = toDate ?? DateTime.Today;

            if (from > to)
            {
                (from, to) = (to, from);
            }

            var model = _repo.GetCollectionsReport(from, to, mode);
            return View(model);
        }

        [Permission("ReportOutstanding", "View")]
        public IActionResult Outstanding(string? status)
        {
            var model = _repo.GetOutstandingReport(status);
            return View(model);
        }

        [Permission("ReportProfitLoss", "View")]
        public IActionResult ProfitLoss(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = toDate ?? DateTime.Today;

            if (from > to)
            {
                (from, to) = (to, from);
            }

            var model = _repo.GetProfitLossReport(from, to);
            return View(model);
        }

        [Permission("ReportAttendance", "View")]
        public IActionResult ExportAttendance() => this.ExportCsv("ReportAttendance");

        [Permission("ReportExpiry", "View")]
        public IActionResult ExportExpiry() => this.ExportCsv("ReportExpiry");

        [Permission("ReportCollections", "View")]
        public IActionResult ExportCollections() => this.ExportCsv("ReportCollections");

        [Permission("ReportOutstanding", "View")]
        public IActionResult ExportOutstanding() => this.ExportCsv("ReportOutstanding");

        [Permission("ReportProfitLoss", "View")]
        public IActionResult ExportProfitLoss() => this.ExportCsv("ReportProfitLoss");
    }
}

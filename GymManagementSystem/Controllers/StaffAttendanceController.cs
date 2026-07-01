using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class StaffAttendanceController : Controller
    {
        private readonly StaffAttendanceRepository _repo;

        public StaffAttendanceController(StaffAttendanceRepository repo)
        {
            _repo = repo;
        }

        [Permission("StaffAttendance", "View")]
        public IActionResult Index(
            string? search,
            string? staffId,
            string? status,
            string? fromDate,
            string? toDate)
        {
            ViewBag.Staff = _repo.GetActiveStaff();

            if (string.IsNullOrWhiteSpace(fromDate) && string.IsNullOrWhiteSpace(toDate))
            {
                fromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                toDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            var attendances = _repo.GetAll(search, staffId, status, fromDate, toDate);

            ViewBag.Search = search;
            ViewBag.StaffId = staffId;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(attendances);
        }

        [Permission("StaffAttendance", "View")]
        public IActionResult History(int? staffId, int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Today.Month;
            var selectedYear = year ?? DateTime.Today.Year;

            var staffList = _repo.GetActiveStaff()
                .OrderBy(s => s.StaffName)
                .Select(s => new StaffAttendanceStaffOption
                {
                    StaffId = s.StaffId,
                    StaffName = s.StaffName,
                    Designation = s.Designation,
                    MobileNo = s.MobileNo,
                    BasicSalary = s.Salary
                })
                .ToList();

            var model = new StaffAttendanceHistoryViewModel
            {
                StaffList = staffList,
                SelectedStaffId = staffId,
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear
            };

            if (staffId.HasValue && staffId > 0)
            {
                model.SelectedStaff = staffList.FirstOrDefault(s => s.StaffId == staffId);
                var monthStart = new DateTime(selectedYear, selectedMonth, 1);
                var monthEnd = new DateTime(
                    selectedYear,
                    selectedMonth,
                    DateTime.DaysInMonth(selectedYear, selectedMonth));

                model.SelectedMonthBlock = BuildMonthBlock(
                    staffId.Value, monthStart, monthEnd, selectedMonth, selectedYear);
            }

            return View(model);
        }

        private StaffMonthAttendanceBlock BuildMonthBlock(int staffId, DateTime from, DateTime to, int month, int year)
        {
            var days = _repo.GetByStaffAndDateRange(staffId, from, to);
            var counts = _repo.GetMonthlyCounts(staffId, month, year);
            var totalMinutes = days
                .Where(d => d.CheckInTime != null && d.CheckOutTime != null)
                .Sum(d => (d.CheckOutTime!.Value - d.CheckInTime!.Value).TotalMinutes);

            var monthNames = new[] { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            return new StaffMonthAttendanceBlock
            {
                Month = month,
                Year = year,
                MonthLabel = $"{monthNames[month]} {year}",
                PresentDays = counts.PresentDays,
                AbsentDays = counts.AbsentDays,
                LeaveDays = counts.LeaveDays,
                HalfDays = counts.HalfDays,
                TotalMarked = counts.TotalMarked,
                TotalWorkTime = FormatDuration(totalMinutes),
                Days = days
            };
        }

        private static string FormatDuration(double totalMinutes)
        {
            if (totalMinutes <= 0) return "0h 0m";
            return $"{(int)(totalMinutes / 60)}h {(int)(totalMinutes % 60)}m";
        }

        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffAttendance", "Add")]
        public IActionResult Save(StaffAttendance model)
        {
            if (string.IsNullOrWhiteSpace(model.Status))
                model.Status = "Present";

            if (_repo.ExistsForStaffDate(model.StaffId, model.AttendanceDate))
            {
                TempData["Error"] = "Attendance already exists for this staff on the selected date.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Insert(model);
            TempData["Success"] = "Staff attendance saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffAttendance", "Add")]
        public IActionResult QuickCheckIn(int staffId, string? returnUrl)
        {
            _repo.QuickCheckIn(staffId);
            TempData["Success"] = "Check-in recorded.";
            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffAttendance", "Edit")]
        public IActionResult QuickCheckOut(int staffId, string? returnUrl)
        {
            _repo.QuickCheckOut(staffId);
            TempData["Success"] = "Check-out recorded.";
            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffAttendance", "Add")]
        public IActionResult MarkAbsent(int staffId, string? remarks, string? returnUrl)
        {
            _repo.MarkTodayStatus(staffId, "Absent", remarks);
            TempData["Success"] = "Marked absent for today.";
            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffAttendance", "Add")]
        public IActionResult MarkStatus(int staffId, string status, string? remarks, string? returnUrl)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Absent", "Leave", "HalfDay"
            };

            if (!allowed.Contains(status))
            {
                TempData["Error"] = "Invalid attendance status.";
                return RedirectToAction(nameof(Index));
            }

            _repo.MarkTodayStatus(staffId, status, remarks);
            TempData["Success"] = $"Marked {status.ToLowerInvariant()} for today.";
            return RedirectBack(returnUrl);
        }

        [Permission("StaffAttendance", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Staff = _repo.GetActiveStaff();
            return PartialView("_EditStaffAttendance", _repo.GetById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffAttendance", "Edit")]
        public IActionResult Update(StaffAttendance model)
        {
            if (_repo.ExistsForStaffDate(model.StaffId, model.AttendanceDate, model.AttendanceId))
            {
                TempData["Error"] = "Another attendance record already exists for this staff on the selected date.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Update(model);
            TempData["Success"] = "Attendance updated.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("StaffAttendance", "Delete")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "Attendance deleted.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("StaffAttendance", "View")]
        public IActionResult Export() => this.ExportCsv("StaffAttendance");
    }
}

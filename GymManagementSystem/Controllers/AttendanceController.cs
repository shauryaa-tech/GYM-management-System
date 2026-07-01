using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;
using GymManagement.ViewModels;

namespace GymManagement.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AttendanceRepository _repo;
        private readonly MemberRepository _memberRepo;

        public AttendanceController(AttendanceRepository repo, MemberRepository memberRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
        }

        [Permission("Attendance", "View")]
        public IActionResult Index(DateTime? filterDate)
        {
            ViewBag.Members = _memberRepo.GetAll(null, "Active");
            
            var attendances = _repo.GetAll(filterDate);

            ViewBag.FilterDate = filterDate ?? DateTime.Today;

            return View(attendances);
        }

        [Permission("Attendance", "View")]
        public IActionResult History(int? memberId)
        {
            var members = _memberRepo.GetAll(null, null)
                .OrderBy(m => m.MemberName)
                .Select(m => new MemberAttendanceMemberOption
                {
                    MemberId = m.MemberId,
                    MemberCode = m.MemberCode,
                    MemberName = m.MemberName,
                    MobileNo = m.MobileNo,
                    PlanName = m.PlanName
                })
                .ToList();

            var model = new MemberAttendanceHistoryViewModel
            {
                Members = members,
                SelectedMemberId = memberId
            };

            if (memberId.HasValue && memberId > 0)
            {
                model.SelectedMember = members.FirstOrDefault(m => m.MemberId == memberId);
                var today = DateTime.Today;
                var currentStart = new DateTime(today.Year, today.Month, 1);
                var currentEnd = today;
                var prevMonth = today.AddMonths(-1);
                var prevStart = new DateTime(prevMonth.Year, prevMonth.Month, 1);
                var prevEnd = new DateTime(prevMonth.Year, prevMonth.Month, DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));

                model.CurrentMonth = BuildMonthBlock(memberId.Value, currentStart, currentEnd, today.Month, today.Year);
                model.PreviousMonth = BuildMonthBlock(memberId.Value, prevStart, prevEnd, prevMonth.Month, prevMonth.Year);
            }

            return View(model);
        }

        private MemberMonthAttendanceBlock BuildMonthBlock(int memberId, DateTime from, DateTime to, int month, int year)
        {
            var records = _repo.GetByMemberAndDateRange(memberId, from, to);
            var days = records.Select(r => new MemberAttendanceDayRow
            {
                AttendanceId = r.AttendanceId,
                AttendanceDate = r.AttendanceDate,
                CheckInTime = r.CheckInTime,
                CheckOutTime = r.CheckOutTime,
                Remarks = r.Remarks
            }).ToList();

            var totalMinutes = days
                .Where(d => d.CheckInTime != null && d.CheckOutTime != null)
                .Sum(d => (d.CheckOutTime!.Value - d.CheckInTime!.Value).TotalMinutes);

            var monthNames = new[] { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            return new MemberMonthAttendanceBlock
            {
                Month = month,
                Year = year,
                MonthLabel = $"{monthNames[month]} {year}",
                TotalVisitDays = days.Count,
                CompletedSessions = days.Count(d => d.CheckInTime != null && d.CheckOutTime != null),
                TotalGymTime = FormatDuration(totalMinutes),
                Days = days
            };
        }

        private static string FormatDuration(double totalMinutes)
        {
            if (totalMinutes <= 0) return "0h 0m";
            var hours = (int)(totalMinutes / 60);
            var mins = (int)(totalMinutes % 60);
            return $"{hours}h {mins}m";
        }

        [HttpPost]
        [Permission("Attendance", "Add")]
        public IActionResult Save(Attendance model)
        {
            try
            {
                if (model.AttendanceDate == DateTime.MinValue)
                {
                    model.AttendanceDate = DateTime.Today;
                }

                _repo.Insert(model);
                TempData["Success"] = "Attendance Marked Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Attendance", "Edit")]
        public IActionResult Edit(int id)
        {
            var attendance =
                _repo.GetById(id);

            ViewBag.Members =
                _memberRepo.GetAll(
                    null,
                    "Active");

            return PartialView(
                "_EditAttendance",
                attendance);
        }

        [HttpPost]
        [Permission("Attendance", "Edit")]
        public IActionResult Update(
    Attendance model)
        {
            try
            {
                _repo.Update(model);

                TempData["Success"] =
                    "Attendance Updated Successfully";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Attendance", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Attendance Record Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Attendance", "View")]
        public IActionResult Export() => this.ExportCsv("Attendance");
    }
}

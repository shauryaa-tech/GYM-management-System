using GymManagement.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardRepository _repo;
        private readonly RolePermissionRepository _permissionRepo;
        private readonly NotificationRepository _notificationRepo;
        private readonly NavbarSearchRepository _searchRepo;
        private readonly StaffAttendanceRepository _staffAttendanceRepo;

        public DashboardController(
            DashboardRepository repo,
            RolePermissionRepository permissionRepo,
            NotificationRepository notificationRepo,
            NavbarSearchRepository searchRepo,
            StaffAttendanceRepository staffAttendanceRepo)
        {
            _repo = repo;
            _permissionRepo = permissionRepo;
            _notificationRepo = notificationRepo;
            _searchRepo = searchRepo;
            _staffAttendanceRepo = staffAttendanceRepo;
        }

        [Permission("Dashboard", "View")]
        public IActionResult Index(DateTime? date)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null)
                return RedirectToAction("Login", "Account");

            var selectedDate = (date ?? DateTime.Today).Date;
            if (selectedDate > DateTime.Today)
                selectedDate = DateTime.Today;

            var data = _repo.GetDashboardData(selectedDate);
            data.FullName = HttpContext.Session.GetString("FullName");

            if (PermissionHelper.CanView(HttpContext, _permissionRepo, "StaffAttendance"))
            {
                data.ShowStaffBoard = true;
                data.StaffTodayBoard = _staffAttendanceRepo.GetBoardForDate(selectedDate);
            }

            return View(data);
        }

        [HttpGet]
        public JsonResult GetNavbarStats()
        {
            if (HttpContext.Session.GetInt32("RoleId") == null)
                return Json(new { error = "Unauthorized" });

            var stats = _repo.GetNavbarStats();
            var notifications = _notificationRepo.GetRecentNotifications(10);
            var notificationCount = _notificationRepo.GetUnreadCount();

            return Json(new
            {
                totalMembers = stats.TotalMembers,
                activeMembers = stats.ActiveMembers,
                todayRevenue = stats.TodayRevenue,
                pendingNotifications = notificationCount,
                notifications = notifications.Select(n => new
                {
                    id = n.Id,
                    type = n.Type,
                    title = n.Title,
                    message = n.Message,
                    icon = n.Icon,
                    color = n.Color,
                    url = n.Url,
                    time = n.Time.ToString("hh:mm tt"),
                    date = n.Time.ToString("dd MMM")
                })
            });
        }

        [HttpGet]
        public JsonResult GetNavbarMessages()
        {
            if (HttpContext.Session.GetInt32("RoleId") == null)
                return Json(new { error = "Unauthorized" });

            var messages = _notificationRepo.GetFollowUpMessages(10);
            var count = _notificationRepo.GetFollowUpCount();

            return Json(new
            {
                count,
                messages = messages.Select(m => new
                {
                    id = m.LeadId,
                    title = m.Title,
                    message = m.Message,
                    status = m.Status,
                    isOverdue = m.IsOverdue,
                    url = m.Url,
                    date = m.Time.ToString("dd MMM yyyy")
                })
            });
        }

        [HttpGet]
        public JsonResult GlobalSearch(string q)
        {
            if (HttpContext.Session.GetInt32("RoleId") == null)
                return Json(new { error = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(new { results = Array.Empty<object>() });

            var results = _searchRepo.Search(q.Trim());

            return Json(new
            {
                results = results.Select(r => new
                {
                    type = r.Type,
                    id = r.Id,
                    title = r.Title,
                    subtitle = r.Subtitle,
                    url = r.Url,
                    icon = r.Icon
                })
            });
        }
    }
}

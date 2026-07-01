using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class ClassBookingController : Controller
    {
        private readonly ClassBookingRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public ClassBookingController(
            ClassBookingRepository repo,
            MemberRepository memberRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("ClassBookings", "View")]
        public IActionResult Index(string? search, string? memberId, string? classId, string? status)
        {
            ViewBag.Members = _memberRepo.GetAll("", "").Where(x => x.Status == "Active").ToList();
            ViewBag.Classes = _repo.GetActiveClasses();

            var bookings = _repo.GetAll(search, memberId, classId, status);

            ViewBag.Search = search;
            ViewBag.MemberId = memberId;
            ViewBag.ClassId = classId;
            ViewBag.Status = status;

            return View(bookings);
        }

        [HttpPost]
        [Permission("ClassBookings", "Add")]
        public IActionResult Save(ClassBooking model)
        {
            try
            {
                var existingCount = _repo.GetBookingCount(model.ClassId, model.BookingDate);
                var selectedClass = _repo.GetActiveClasses().FirstOrDefault(c => c.ClassId == model.ClassId);

                if (selectedClass != null && selectedClass.MaxCapacity.HasValue && existingCount >= selectedClass.MaxCapacity.Value)
                {
                    TempData["Error"] = "Class is fully booked for this date.";
                    return RedirectToAction("Index");
                }

                if (model.Amount == null && selectedClass != null)
                {
                    model.Amount = selectedClass.Amount;
                }

                _repo.Insert(model);
                TempData["Success"] = "Booking Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("ClassBookings", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Members = _memberRepo.GetAll("", "").Where(x => x.Status == "Active").ToList();
            ViewBag.Classes = _repo.GetActiveClasses();
            var model = _repo.GetById(id);
            return PartialView("_EditBooking", model);
        }

        [HttpPost]
        [Permission("ClassBookings", "Edit")]
        public IActionResult Update(ClassBooking model)
        {
            try
            {
                var existingCount = _repo.GetBookingCount(model.ClassId, model.BookingDate);
                var selectedClass = _repo.GetActiveClasses().FirstOrDefault(c => c.ClassId == model.ClassId);

                if (selectedClass != null && selectedClass.MaxCapacity.HasValue && existingCount >= selectedClass.MaxCapacity.Value)
                {
                    var currentBooking = _repo.GetById(model.BookingId);
                    if (currentBooking.ClassId != model.ClassId || currentBooking.BookingDate != model.BookingDate)
                    {
                        TempData["Error"] = "Class is fully booked for this date.";
                        return RedirectToAction("Index");
                    }
                }

                _repo.Update(model);
                TempData["Success"] = "Booking Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("ClassBookings", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Booking Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        [Permission("ClassBookings", "Edit")]
        public IActionResult CancelBooking(int id)
        {
            try
            {
                var booking = _repo.GetById(id);
                if (booking != null)
                {
                    booking.Status = "Cancelled";
                    _repo.Update(booking);
                    TempData["Success"] = "Booking Cancelled Successfully";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("ClassBookings", "View")]
        public IActionResult Export() => this.ExportCsv("ClassBookings");
    }
}
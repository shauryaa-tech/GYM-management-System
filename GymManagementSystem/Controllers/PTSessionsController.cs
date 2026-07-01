using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class PTSessionsController : Controller
    {
        private readonly PTSessionRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly StaffRepository _staffRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public PTSessionsController(
            PTSessionRepository repo,
            MemberRepository memberRepo,
            StaffRepository staffRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _staffRepo = staffRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("PTSessions", "View")]
        public IActionResult Index(string? search)
        {
            LoadDropdowns();

            var list = _repo.GetAll(search);

            ViewBag.Search = search;

            return View(list);
        }

        [HttpPost]
        [Permission("PTSessions", "Add")]
        public IActionResult Save(PTSession model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Status))
                {
                    model.Status = "Scheduled";
                }

                _repo.Insert(model);

                TempData["Success"] = "PT Session Added Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("PTSessions", "Edit")]
        public IActionResult Edit(int id)
        {
            LoadDropdowns();

            var model = _repo.GetById(id);

            return PartialView("_EditSession", model);
        }

        [HttpPost]
        [Permission("PTSessions", "Edit")]
        public IActionResult Update(PTSession model)
        {
            try
            {
                _repo.Update(model);

                TempData["Success"] = "PT Session Updated Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("PTSessions", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);

                TempData["Success"] = "PT Session Deleted Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("PTSessions", "View")]
        public IActionResult Export() => this.ExportCsv("PTSessions");

        private void LoadDropdowns()
        {
            ViewBag.Members = _memberRepo
                .GetAll("", "")
                .Where(x => x.Status == "Active")
                .ToList();

            ViewBag.Trainers = _staffRepo
                .GetAll()
                .Where(x =>
                    x.IsActive &&
                    x.Designation != null &&
                    x.Designation.Contains("Trainer", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}

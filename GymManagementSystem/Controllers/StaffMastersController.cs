using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class StaffMastersController : Controller
    {
        private readonly StaffRepository _repo;
        private readonly RoleRepository _roleRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public StaffMastersController(
            StaffRepository repo,
            RoleRepository roleRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _roleRepo = roleRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("StaffMaster", "View")]
        public IActionResult Index()
        {
            ViewBag.Roles =
                _roleRepo.GetAll();

            var staff =
                _repo.GetAll();

            return View(staff);
        }

        [HttpPost]
        [Permission("StaffMaster", "Add")]
        public IActionResult Save(
            StaffMaster staff,
            string[]? SpecializationOptions)
        {
            try
            {
                if (SpecializationOptions?.Length > 0)
                    staff.Specializations = string.Join(", ", SpecializationOptions);

                if (!string.IsNullOrWhiteSpace(staff.StaffCode) &&
                    _repo.IsStaffCodeTaken(staff.StaffCode))
                {
                    TempData["Error"] = $"Staff Code '{staff.StaffCode}' already exists. Use a unique code for biometric.";
                    return RedirectToAction("Index");
                }

                _repo.Insert(staff);

                TempData["Success"] =
                    "Staff Added Successfully";

                return RedirectToAction(
                    "Index");
            }
            catch (Exception ex)
            {
                return Content(
                    ex.ToString());
            }
        }

        [Permission("StaffMaster", "Edit")]
        public IActionResult Edit(int id)
        {
            var staff =
                _repo.GetById(id);

            ViewBag.Roles =
                _roleRepo.GetAll();

            return PartialView(
                "_EditStaff",
                staff);
        }

        [HttpPost]
        [Permission("StaffMaster", "Edit")]
        public IActionResult Update(
            StaffMaster staff,
            string[]? SpecializationOptions)
        {
            try
            {
                if (SpecializationOptions?.Length > 0)
                    staff.Specializations = string.Join(", ", SpecializationOptions);

                if (!string.IsNullOrWhiteSpace(staff.StaffCode) &&
                    _repo.IsStaffCodeTaken(staff.StaffCode, staff.StaffId))
                {
                    TempData["Error"] = $"Staff Code '{staff.StaffCode}' already exists.";
                    return RedirectToAction("Index");
                }

                _repo.Update(staff);

                TempData["Success"] =
                    "Staff Updated Successfully";

                return RedirectToAction(
                    "Index");
            }
            catch (Exception ex)
            {
                return Content(
                    ex.ToString());
            }
        }

        [Permission("StaffMaster", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);

                TempData["Success"] =
                    "Staff Deleted Successfully";

                return RedirectToAction(
                    "Index");
            }
            catch (Exception ex)
            {
                return Content(
                    ex.ToString());
            }
        }

        [Permission("StaffMaster", "View")]
        public IActionResult Export() => this.ExportCsv("StaffMaster");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("StaffMaster", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("StaffMaster", file);
    }
}
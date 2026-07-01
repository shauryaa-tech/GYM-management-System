using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class ClassMasterController : Controller
    {
        private readonly ClassRepository _repo;
        private readonly StaffRepository _staffRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public ClassMasterController(
            ClassRepository repo,
            StaffRepository staffRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _staffRepo = staffRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("Classes", "View")]
        public IActionResult Index(string? search, string? trainerId)
        {
            ViewBag.Trainers = _staffRepo.GetAll().Where(x => x.RoleId == 2).ToList();
            
            var classes = _repo.GetAll(search, trainerId);

            ViewBag.Search = search;
            ViewBag.TrainerId = trainerId;

            return View(classes);
        }

        [HttpPost]
        [Permission("Classes", "Add")]
        public IActionResult Save(ClassMaster model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Class Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Classes", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Trainers = _staffRepo.GetAll().Where(x => x.RoleId == 2).ToList();
            var model = _repo.GetById(id);
            return PartialView("_EditClass", model);
        }

        [HttpPost]
        [Permission("Classes", "Edit")]
        public IActionResult Update(ClassMaster model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Class Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Classes", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Class Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Classes", "View")]
        public IActionResult Export() => this.ExportCsv("Classes");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Classes", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("Classes", file);
    }
}

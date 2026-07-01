using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class DietMasterController : Controller
    {
        private readonly DietRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public DietMasterController(
            DietRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("DietMaster", "View")]
        public IActionResult Index(string? search, string? category)
        {
            var diets = _repo.GetAll(search, category);

            ViewBag.Search = search;
            ViewBag.Category = category;

            return View(diets);
        }

        [HttpPost]
        [Permission("DietMaster", "Add")]
        public IActionResult Save(DietMaster diet)
        {
            try
            {
                _repo.Insert(diet);
                TempData["Success"] = "Diet Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("DietMaster", "Edit")]
        public IActionResult Edit(int id)
        {
            var diet = _repo.GetById(id);
            return PartialView("_EditDiet", diet);
        }

        [HttpPost]
        [Permission("DietMaster", "Edit")]
        public IActionResult Update(DietMaster diet)
        {
            try
            {
                _repo.Update(diet);
                TempData["Success"] = "Diet Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("DietMaster", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Diet Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("DietMaster", "View")]
        public IActionResult Export() => this.ExportCsv("DietMaster");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("DietMaster", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("DietMaster", file);
    }
}

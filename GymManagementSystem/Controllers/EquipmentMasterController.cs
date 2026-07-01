using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class EquipmentMasterController : Controller
    {
        private readonly EquipmentRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public EquipmentMasterController(
            EquipmentRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("Equipment", "View")]
        public IActionResult Index(string? search, string? conditionStatus)
        {
            var equipments = _repo.GetAll(search, conditionStatus);

            ViewBag.Search = search;
            ViewBag.ConditionStatus = conditionStatus;

            return View(equipments);
        }

        [HttpPost]
        [Permission("Equipment", "Add")]
        public IActionResult Save(EquipmentMaster model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Equipment Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Equipment", "Edit")]
        public IActionResult Edit(int id)
        {
            var model = _repo.GetById(id);
            return PartialView("_EditEquipment", model);
        }

        [HttpPost]
        [Permission("Equipment", "Edit")]
        public IActionResult Update(EquipmentMaster model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Equipment Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Equipment", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Equipment Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Equipment", "View")]
        public IActionResult Export() => this.ExportCsv("Equipment");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Equipment", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("Equipment", file);
    }
}

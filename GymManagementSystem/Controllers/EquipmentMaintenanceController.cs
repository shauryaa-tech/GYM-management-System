using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class EquipmentMaintenanceController : Controller
    {
        private readonly EquipmentMaintenanceRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public EquipmentMaintenanceController(
            EquipmentMaintenanceRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("EquipmentMaintenance", "View")]
        public IActionResult Index(string? search, string? equipmentId, string? status, string? fromDate, string? toDate)
        {
            ViewBag.Equipment = _repo.GetActiveEquipment();

            var list = _repo.GetAll(search, equipmentId, status, fromDate, toDate);

            ViewBag.Search = search;
            ViewBag.EquipmentId = equipmentId;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(list);
        }

        [HttpPost]
        [Permission("EquipmentMaintenance", "Add")]
        public IActionResult Save(EquipmentMaintenance model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Equipment Maintenance Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("EquipmentMaintenance", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Equipment = _repo.GetActiveEquipment();
            var model = _repo.GetById(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [Permission("EquipmentMaintenance", "Edit")]
        public IActionResult Update(EquipmentMaintenance model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Equipment Maintenance Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("EquipmentMaintenance", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Equipment Maintenance Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("EquipmentMaintenance", "View")]
        public IActionResult Export() => this.ExportCsv("EquipmentMaintenance");
    }
}

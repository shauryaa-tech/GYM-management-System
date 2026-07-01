using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class VendorsController : Controller
    {
        private readonly VendorRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public VendorsController(
            VendorRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("Vendors", "View")]
        public IActionResult Index(string? search)
        {
            var vendors = _repo.GetAll(search);

            ViewBag.Search = search;

            return View(vendors);
        }

        [HttpPost]
        [Permission("Vendors", "Add")]
        public IActionResult Save(VendorMaster model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Vendor Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Vendors", "Edit")]
        public IActionResult Edit(int id)
        {
            var model = _repo.GetById(id);
            return PartialView("_EditVendor", model);
        }

        [HttpPost]
        [Permission("Vendors", "Edit")]
        public IActionResult Update(VendorMaster model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Vendor Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Vendors", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Vendor Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Vendors", "View")]
        public IActionResult Export() => this.ExportCsv("Vendors");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Vendors", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("Vendors", file);
    }
}

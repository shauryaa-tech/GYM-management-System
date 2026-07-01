using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductRepository _repo;
        private readonly VendorRepository _vendorRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public ProductsController(
            ProductRepository repo,
            VendorRepository vendorRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _vendorRepo = vendorRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("Products", "View")]
        public IActionResult Index(string? search, string? category)
        {
            ViewBag.Vendors = _vendorRepo.GetAll(null).Where(x => x.IsActive).ToList();
            var products = _repo.GetAll(search, category);

            ViewBag.Search = search;
            ViewBag.Category = category;

            return View(products);
        }

        [HttpPost]
        [Permission("Products", "Add")]
        public IActionResult Save(ProductMaster model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Product Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Products", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Vendors = _vendorRepo.GetAll(null).Where(x => x.IsActive).ToList();
            var model = _repo.GetById(id);
            return PartialView("_EditProduct", model);
        }

        [HttpPost]
        [Permission("Products", "Edit")]
        public IActionResult Update(ProductMaster model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Product Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Products", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Product Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Products", "View")]
        public IActionResult Export() => this.ExportCsv("Products");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Products", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("Products", file);
    }
}

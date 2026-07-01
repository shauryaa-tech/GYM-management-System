using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class StockPurchaseController : Controller
    {
        private readonly StockPurchaseRepository _repo;
        private readonly ProductRepository _productRepo;
        private readonly VendorRepository _vendorRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public StockPurchaseController(
            StockPurchaseRepository repo,
            ProductRepository productRepo,
            VendorRepository vendorRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _productRepo = productRepo;
            _vendorRepo = vendorRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("StockPurchase", "View")]
        public IActionResult Index(string? search, string? productId, string? vendorId, string? fromDate, string? toDate)
        {
            ViewBag.Products = _repo.GetActiveProducts();
            ViewBag.Vendors = _repo.GetActiveVendors();

            var purchases = _repo.GetAll(search, productId, vendorId, fromDate, toDate);

            ViewBag.Search = search;
            ViewBag.ProductId = productId;
            ViewBag.VendorId = vendorId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(purchases);
        }

        [HttpPost]
        [Permission("StockPurchase", "Add")]
        public IActionResult Save(StockPurchase model)
        {
            try
            {
                if (model.TotalAmount == 0)
                {
                    model.TotalAmount = model.Quantity * model.UnitPrice;
                }

                _repo.Insert(model);
                TempData["Success"] = "Stock Purchase Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("StockPurchase", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Products = _repo.GetActiveProducts();
            ViewBag.Vendors = _repo.GetActiveVendors();
            var model = _repo.GetById(id);
            return PartialView("_EditPurchase", model);
        }

        [HttpPost]
        [Permission("StockPurchase", "Edit")]
        public IActionResult Update(StockPurchase model)
        {
            try
            {
                if (model.TotalAmount == 0)
                {
                    model.TotalAmount = model.Quantity * model.UnitPrice;
                }

                _repo.Update(model);
                TempData["Success"] = "Stock Purchase Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("StockPurchase", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Stock Purchase Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpGet]
        public JsonResult GetProductPrice(int productId)
        {
            var product = _repo.GetActiveProducts().FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                return Json(new { unitPrice = product.UnitPrice, currentStock = product.CurrentStock });
            }
            return Json(new { unitPrice = 0, currentStock = 0 });
        }

        [Permission("StockPurchase", "View")]
        public IActionResult Export() => this.ExportCsv("StockPurchase");
    }
}
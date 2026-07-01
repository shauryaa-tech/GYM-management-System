using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class StockIssueController : Controller
    {
        private readonly StockIssueRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public StockIssueController(
            StockIssueRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("StockIssue", "View")]
        public IActionResult Index(string? search, string? productId, string? memberId, string? fromDate, string? toDate)
        {
            ViewBag.Products = _repo.GetActiveProducts();
            ViewBag.Members = _repo.GetActiveMembers();

            var issues = _repo.GetAll(search, productId, memberId, fromDate, toDate);

            ViewBag.Search = search;
            ViewBag.ProductId = productId;
            ViewBag.MemberId = memberId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(issues);
        }

        [HttpPost]
        [Permission("StockIssue", "Add")]
        public IActionResult Save(StockIssue model)
        {
            try
            {
                if (model.Amount == null)
                {
                    var product = _repo.GetActiveProducts().FirstOrDefault(p => p.ProductId == model.ProductId);
                    if (product != null)
                    {
                        model.Amount = model.Quantity * product.UnitPrice;
                    }
                }

                if (string.IsNullOrWhiteSpace(model.IssuedTo) && model.MemberId.HasValue)
                {
                    var member = _repo.GetActiveMembers().FirstOrDefault(m => m.MemberId == model.MemberId);
                    model.IssuedTo = member?.MemberName ?? "";
                }

                _repo.Insert(model);
                TempData["Success"] = "Stock Issue Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("StockIssue", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Products = _repo.GetActiveProducts();
            ViewBag.Members = _repo.GetActiveMembers();
            var model = _repo.GetById(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [Permission("StockIssue", "Edit")]
        public IActionResult Update(StockIssue model)
        {
            try
            {
                if (model.Amount == null)
                {
                    var product = _repo.GetActiveProducts().FirstOrDefault(p => p.ProductId == model.ProductId);
                    if (product != null)
                    {
                        model.Amount = model.Quantity * product.UnitPrice;
                    }
                }

                if (string.IsNullOrWhiteSpace(model.IssuedTo) && model.MemberId.HasValue)
                {
                    var member = _repo.GetActiveMembers().FirstOrDefault(m => m.MemberId == model.MemberId);
                    model.IssuedTo = member?.MemberName ?? "";
                }

                _repo.Update(model);
                TempData["Success"] = "Stock Issue Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("StockIssue", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Stock Issue Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpGet]
        public JsonResult GetProductDetails(int productId)
        {
            var product = _repo.GetActiveProducts().FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                return Json(new { unitPrice = product.UnitPrice, currentStock = product.CurrentStock });
            }
            return Json(new { unitPrice = 0, currentStock = 0 });
        }

        [Permission("StockIssue", "View")]
        public IActionResult Export() => this.ExportCsv("StockIssue");
    }
}

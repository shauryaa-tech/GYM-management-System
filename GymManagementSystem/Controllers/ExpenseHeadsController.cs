using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class ExpenseHeadsController : Controller
    {
        private readonly ExpenseHeadRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public ExpenseHeadsController(
            ExpenseHeadRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("ExpenseHeads", "View")]
        public IActionResult Index(string? search)
        {
            var heads = _repo.GetAll(search);

            ViewBag.Search = search;

            return View(heads);
        }

        [HttpPost]
        [Permission("ExpenseHeads", "Add")]
        public IActionResult Save(ExpenseHeadMaster model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Expense Head Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("ExpenseHeads", "Edit")]
        public IActionResult Edit(int id)
        {
            var model = _repo.GetById(id);
            return PartialView("_EditExpenseHead", model);
        }

        [HttpPost]
        [Permission("ExpenseHeads", "Edit")]
        public IActionResult Update(ExpenseHeadMaster model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Expense Head Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("ExpenseHeads", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Expense Head Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("ExpenseHeads", "View")]
        public IActionResult Export() => this.ExportCsv("ExpenseHeads");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("ExpenseHeads", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("ExpenseHeads", file);
    }
}

using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ExpenseRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public ExpensesController(
            ExpenseRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("Expenses", "View")]
        public IActionResult Index(string? search, string? expenseHeadId, string? paymentMode, string? fromDate, string? toDate)
        {
            ViewBag.ExpenseHeads = _repo.GetActiveExpenseHeads();

            var expenses = _repo.GetAll(search, expenseHeadId, paymentMode, fromDate, toDate);

            ViewBag.Search = search;
            ViewBag.ExpenseHeadId = expenseHeadId;
            ViewBag.PaymentMode = paymentMode;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(expenses);
        }

        [HttpPost]
        [Permission("Expenses", "Add")]
        public IActionResult Save(Expense model)
        {
            try
            {
                _repo.Insert(model);
                TempData["Success"] = "Expense Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Expenses", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.ExpenseHeads = _repo.GetActiveExpenseHeads();
            var model = _repo.GetById(id);
            return PartialView("_EditExpense", model);
        }

        [HttpPost]
        [Permission("Expenses", "Edit")]
        public IActionResult Update(Expense model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Expense Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Expenses", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Expense Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Expenses", "View")]
        public IActionResult Export() => this.ExportCsv("Expenses");
    }
}

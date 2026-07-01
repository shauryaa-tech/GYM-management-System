using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class SalaryRuleMasterController : Controller
    {
        private readonly SalaryRuleMasterRepository _repo;

        public SalaryRuleMasterController(SalaryRuleMasterRepository repo)
        {
            _repo = repo;
        }

        [Permission("SalaryRuleMaster", "View")]
        public IActionResult Index(string? search)
        {
            ViewBag.Search = search;
            return View(_repo.GetAll(search));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryRuleMaster", "Add")]
        public IActionResult Save(SalaryRuleMaster model, string[]? WeeklyOffDayOptions)
        {
            if (string.IsNullOrWhiteSpace(model.RuleName))
            {
                TempData["Error"] = "Rule name is required.";
                return RedirectToAction(nameof(Index));
            }

            if (model.WorkingDaysPerMonth <= 0)
                model.WorkingDaysPerMonth = 26;

            if (WeeklyOffDayOptions?.Length > 0)
                model.WeeklyOffDays = string.Join(",", WeeklyOffDayOptions);

            _repo.Insert(model);
            TempData["Success"] = "Salary rule added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("SalaryRuleMaster", "Edit")]
        public IActionResult Edit(int id)
        {
            var model = _repo.GetById(id);
            if (model == null) return NotFound();
            return PartialView("_EditSalaryRule", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryRuleMaster", "Edit")]
        public IActionResult Update(SalaryRuleMaster model, string[]? WeeklyOffDayOptions)
        {
            if (string.IsNullOrWhiteSpace(model.RuleName))
            {
                TempData["Error"] = "Rule name is required.";
                return RedirectToAction(nameof(Index));
            }

            if (WeeklyOffDayOptions?.Length > 0)
                model.WeeklyOffDays = string.Join(",", WeeklyOffDayOptions);

            _repo.Update(model);
            TempData["Success"] = "Salary rule updated.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("SalaryRuleMaster", "Edit")]
        public IActionResult SetDefault(int id)
        {
            _repo.SetDefault(id);
            TempData["Success"] = "Default salary rule set.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("SalaryRuleMaster", "Delete")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "Salary rule deleted.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("SalaryRuleMaster", "View")]
        public IActionResult Export() => this.ExportCsv("SalaryRuleMaster");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryRuleMaster", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("SalaryRuleMaster", file);
    }
}

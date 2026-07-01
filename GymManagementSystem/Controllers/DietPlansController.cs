using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class DietPlansController : Controller
    {
        private readonly DietPlanRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly StaffRepository _staffRepo;

        public DietPlansController(
            DietPlanRepository repo,
            MemberRepository memberRepo,
            StaffRepository staffRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _staffRepo = staffRepo;
        }

        [Permission("DietPlans", "View")]
        public IActionResult Index(string? search)
        {
            ViewBag.Members = _memberRepo.GetAll(null, "Active");
            ViewBag.Dietitians = _staffRepo.GetAll().Where(x => x.IsActive && x.Designation != null && x.Designation.Contains("Dietitian", StringComparison.OrdinalIgnoreCase)).ToList();

            var plans = _repo.GetAll(search);

            ViewBag.Search = search;

            return View(plans);
        }

        [HttpPost]
        [Permission("DietPlans", "Add")]
        public IActionResult Save(DietPlan model)
        {
            try
            {
                if (model.StartDate == DateTime.MinValue)
                    model.StartDate = DateTime.Today;
                if (model.EndDate == DateTime.MinValue)
                    model.EndDate = DateTime.Today.AddMonths(1);

                _repo.Insert(model);
                TempData["Success"] = "Diet Plan Created Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("DietPlans", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Members = _memberRepo.GetAll(null, "Active");
            ViewBag.Dietitians = _staffRepo.GetAll().Where(x => x.IsActive && x.Designation != null && x.Designation.Contains("Dietitian", StringComparison.OrdinalIgnoreCase)).ToList();

            var model = _repo.GetById(id);
            return PartialView("_EditPlan", model);
        }

        [HttpPost]
        [Permission("DietPlans", "Edit")]
        public IActionResult Update(DietPlan model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Diet Plan Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("DietPlans", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Diet Plan Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("DietPlans", "View")]
        public IActionResult Export() => this.ExportCsv("DietPlans");
    }
}

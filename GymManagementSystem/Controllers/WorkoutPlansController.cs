using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class WorkoutPlansController : Controller
    {
        private readonly WorkoutPlanRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly StaffRepository _staffRepo;

        public WorkoutPlansController(WorkoutPlanRepository repo, MemberRepository memberRepo, StaffRepository staffRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _staffRepo = staffRepo;
        }

        [Permission("WorkoutPlans", "View")]
        public IActionResult Index(string? search)
        {
            ViewBag.Members = _memberRepo.GetAll(null, "Active");
            ViewBag.Trainers = _staffRepo.GetAll().Where(x => x.IsActive && x.Designation != null && x.Designation.Contains("Trainer", StringComparison.OrdinalIgnoreCase)).ToList();

            var plans = _repo.GetAll(search);

            ViewBag.Search = search;

            return View(plans);
        }

        [HttpPost]
        [Permission("WorkoutPlans", "Add")]
        public IActionResult Save(WorkoutPlan model)
        {
            try
            {
                if (model.StartDate == DateTime.MinValue)
                {
                    model.StartDate = DateTime.Today;
                }
                if (model.EndDate == DateTime.MinValue)
                {
                    model.EndDate = DateTime.Today.AddMonths(1);
                }

                _repo.Insert(model);
                TempData["Success"] = "Workout Plan Created Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("WorkoutPlans", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Members = _memberRepo.GetAll(null, "Active");
            ViewBag.Trainers = _staffRepo.GetAll().Where(x => x.IsActive && x.Designation != null && x.Designation.Contains("Trainer", StringComparison.OrdinalIgnoreCase)).ToList();

            var model = _repo.GetById(id);
            return PartialView("_EditPlan", model);
        }

        [HttpPost]
        [Permission("WorkoutPlans", "Edit")]
        public IActionResult Update(WorkoutPlan model)
        {
            try
            {
                _repo.Update(model);
                TempData["Success"] = "Workout Plan Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("WorkoutPlans", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Workout Plan Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("WorkoutPlans", "View")]
        public IActionResult Export() => this.ExportCsv("WorkoutPlans");
    }
}

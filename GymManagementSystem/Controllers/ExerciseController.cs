using Microsoft.AspNetCore.Mvc;
using GymManagement.Repositories;
using GymManagement.Models;
using GymManagement.Helpers;
using GymManagement.Data.Repositories;

namespace GymManagement.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly ExerciseRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public ExerciseController(
            ExerciseRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("ExerciseMaster", "View")]
        public IActionResult Index(
            string? search,
            bool? status)
        {
            var exercises = _repo.GetAll(search, status);

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(exercises);
        }

        [Permission("ExerciseMaster", "Add")]
        public IActionResult Save()
        {
            return View();
        }

        [HttpPost]
        [Permission("ExerciseMaster", "Add")]
        public IActionResult Save(ExerciseMaster model)
        {
            _repo.Insert(model);

            TempData["Success"] =
                "Exercise Added Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Permission("ExerciseMaster", "Edit")]
        public IActionResult Edit(int id)
        {
            var exercise =
                _repo.GetById(id);

            return PartialView(
                "_EditExercise",
                exercise);
        }

        [HttpPost]
        [Permission("ExerciseMaster", "Edit")]
        public IActionResult Edit(ExerciseMaster model)
        {
            _repo.Update(model);

            TempData["Success"] =
                "Exercise Updated Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Permission("ExerciseMaster", "Edit")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);

            TempData["Success"] =
                "Exercise Deleted Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Permission("ExerciseMaster", "View")]
        public IActionResult Export() => this.ExportCsv("ExerciseMaster");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("ExerciseMaster", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("ExerciseMaster", file);
    }
}
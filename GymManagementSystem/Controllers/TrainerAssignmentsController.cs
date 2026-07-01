using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.Controllers
{
    public class TrainerAssignmentsController : Controller
    {
        private readonly TrainerAssignmentRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly StaffRepository _staffRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public TrainerAssignmentsController(
            TrainerAssignmentRepository repo,
            MemberRepository memberRepo,
            StaffRepository staffRepo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _staffRepo = staffRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("TrainerAssignment", "View")]
        public IActionResult Index(
            string? search,
            bool? status)
        {
            var list =
                _repo.GetAll(search, status);

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(list);
        }

        [Permission("TrainerAssignment", "Add")]
        public IActionResult Save()
        {
            LoadDropdowns();

            return View(
                new TrainerAssignmentModel
                {
                    StartDate = DateTime.Today,
                    IsActive = true
                });
        }

        [HttpPost]
        [Permission("TrainerAssignment", "Add")]
        public IActionResult Save(
            TrainerAssignmentModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();

                return View(model);
            }

            _repo.Insert(model);

            TempData["Success"] =
                "Trainer Assigned Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Permission("TrainerAssignment", "Edit")]
        public IActionResult Edit(int id)
        {
            LoadDropdowns();

            var model =
                _repo.GetById(id);

            return PartialView(
                "_EditTrainerAssignment",
                model);
        }

        [HttpPost]
        [Permission("TrainerAssignment", "Edit")]
        public IActionResult Edit(
            TrainerAssignmentModel model)
        {
            _repo.Update(model);

            TempData["Success"] =
                "Trainer Assignment Updated Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Permission("TrainerAssignment", "Delete")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);

            TempData["Success"] =
                "Trainer Assignment Deleted Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Permission("TrainerAssignment", "View")]
        public IActionResult Export() => this.ExportCsv("TrainerAssignment");

        private void LoadDropdowns()
        {
            ViewBag.MemberList =
                _memberRepo
                .GetActiveMembers()
                .Select(x => new SelectListItem
                {
                    Value = x.MemberId.ToString(),
                    Text = x.MemberName
                })
                .ToList();

            ViewBag.TrainerList =
                _staffRepo
                .GetTrainers()
                .Select(x => new SelectListItem
                {
                    Value = x.StaffId.ToString(),
                    Text = x.StaffName
                })
                .ToList();
        }
    }
}
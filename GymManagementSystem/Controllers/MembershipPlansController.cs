using GymManagement.Data.Repositories;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class MembershipPlansController : Controller
    {
        private readonly MembershipRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public MembershipPlansController(
            MembershipRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("MembershipPlans", "View")]
        public IActionResult Index(
            string? search,
            bool? status)
        {
            var plans =
                _repo.GetAll(search, status);

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(plans);
        }

        [HttpPost]
        [Permission("MembershipPlans", "Add")]
        public IActionResult Save(
           MembershipPlanMaster plan)
        {
            _repo.Insert(plan);

            return RedirectToAction("Index");
        }

        [Permission("MembershipPlans", "Edit")]
        public IActionResult Edit(int id)
        {
            var plan = _repo.GetById(id);

            return PartialView("_EditMembershipPlan", plan);
        }


        [HttpPost]
        [Permission("MembershipPlans", "Edit")]
        public IActionResult Update(
            MembershipPlanMaster plan)
        {
            _repo.Update(plan);

            return RedirectToAction("Index");
        }

        [Permission("MembershipPlans", "Delete")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);

            return RedirectToAction("Index");
        }

        [Permission("MembershipPlans", "View")]
        public IActionResult Export() => this.ExportCsv("MembershipPlans");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("MembershipPlans", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("MembershipPlans", file);
    }
}
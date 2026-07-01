using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class MembersController : Controller
    {
        private readonly MembersRepository _repo;

        private readonly RolePermissionRepository _rolePermissionRepo;

        private readonly StaffRepository _staffRepo;

        private readonly MembershipRepository _membershipRepo;

        public MembersController(
        MembersRepository repo,
        StaffRepository staffRepo,
        MembershipRepository membershipRepo,
        RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _staffRepo = staffRepo;
            _membershipRepo = membershipRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("Members", "View")]
        public IActionResult Index(
            string? search,
            string? trainer,
            string? plan,
            string? status)
        {
            var members =
                _repo.GetAll(
                    search,
                    trainer,
                    plan,
                    status);

            ViewBag.Search = search;
            ViewBag.Trainer = trainer;
            ViewBag.Plan = plan;
            ViewBag.Status = status;

            ViewBag.TrainerList =
                _staffRepo.GetTrainers();

            ViewBag.PlanList =
    _membershipRepo.GetAll(null, null);

            return View(members);
        }

        [Permission("Members", "View")]
        public IActionResult Details(int id)
        {
            var member =
                _repo.GetById(id);

            return PartialView(
                "_MemberDetails",
                member);
        }

        [Permission("Members", "View")]
        public IActionResult Export() => this.ExportCsv("Members");
    }
}
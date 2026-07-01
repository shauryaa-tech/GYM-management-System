using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Helpers;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class MemberMastersController : Controller
    {
        private readonly MemberRepository _repo;
        private readonly MembershipRepository _membershipRepo;
        private readonly StaffRepository _staffRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;
        private readonly ITrainerAutoAssignService _trainerAutoAssign;
        private readonly TrainerAssignmentRepository _trainerAssignmentRepo;
        private readonly LeadRepository _leadRepo;

        public MemberMastersController(
            MemberRepository repo,
            MembershipRepository membershipRepo,
            StaffRepository staffRepo,
            RolePermissionRepository rolePermissionRepo,
            ITrainerAutoAssignService trainerAutoAssign,
            TrainerAssignmentRepository trainerAssignmentRepo,
            LeadRepository leadRepo)
        {
            _repo = repo;
            _membershipRepo = membershipRepo;
            _staffRepo = staffRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _trainerAutoAssign = trainerAutoAssign;
            _trainerAssignmentRepo = trainerAssignmentRepo;
            _leadRepo = leadRepo;
        }

        [Permission("MemberMaster", "View")]
        public IActionResult Index(string? search, string? status)
        {
            ViewBag.Plans = _membershipRepo.GetAll(null, null);

            ViewBag.Trainers =
                    _staffRepo.GetAll()
                    .Where(x => x.RoleId == 2)
                    .ToList();

                var members =
                    _repo.GetAll(search, status);

                ViewBag.Search = search;
                ViewBag.Status = status;

                return View(members);
            }

        [HttpPost]
        [Permission("MemberMaster", "Add")]
        public IActionResult Save(MemberMaster member, string? InterestedIn)
        {
            try
            {
                if (!member.TrainerId.HasValue || member.TrainerId <= 0)
                {
                    var interest = !string.IsNullOrWhiteSpace(InterestedIn)
                        ? InterestedIn
                        : _leadRepo.GetLatestInterestedInByMobile(member.MobileNo);

                    var trainerId = _trainerAutoAssign.FindBestTrainerId(interest);
                    if (trainerId.HasValue)
                        member.TrainerId = trainerId;
                }

                var memberId = _repo.Insert(member);

                if (member.TrainerId.HasValue && member.TrainerId > 0)
                {
                    _trainerAssignmentRepo.Insert(new TrainerAssignmentModel
                    {
                        MemberId = memberId,
                        TrainerId = member.TrainerId.Value,
                        StartDate = DateTime.Today,
                        IsActive = true,
                        Remarks = "Auto-assigned based on member interest"
                    });
                }

                var trainerName = member.TrainerId.HasValue
                    ? _trainerAutoAssign.GetTrainerName(member.TrainerId.Value)
                    : null;

                TempData["Success"] = trainerName != null
                    ? $"Member Added Successfully. Trainer auto-assigned: {trainerName}"
                    : "Member Added Successfully";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }


        [Permission("MemberMaster", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Plans = _membershipRepo.GetAll(null, null);

            ViewBag.Trainers = _staffRepo.GetAll()
                .Where(x => x.RoleId == 2)
                .ToList();

            var member = _repo.GetById(id);

            return PartialView("_EditMember", member);
        }


        [HttpPost]
        [Permission("MemberMaster", "Edit")]
        public IActionResult Edit(MemberMaster member)
        {
            try
            {
                _repo.Update(member);

                TempData["Success"] =
                    "Member Updated Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }


        [Permission("MemberMaster", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);

                TempData["Success"] =
                    "Member Deleted Successfully";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("MemberMaster", "View")]
        public IActionResult Export() => this.ExportCsv("MemberMaster");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("MemberMaster", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("MemberMaster", file);
    }
}
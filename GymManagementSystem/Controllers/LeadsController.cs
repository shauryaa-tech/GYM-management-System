using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class LeadsController : Controller
    {
        private readonly LeadRepository _repo;
        private readonly LeadSourceRepository _leadSourceRepo;
        private readonly StaffRepository _staffRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;
        private readonly ITrainerAutoAssignService _trainerAutoAssign;

        public LeadsController(
            LeadRepository repo,
            LeadSourceRepository leadSourceRepo,
            StaffRepository staffRepo,
            RolePermissionRepository rolePermissionRepo,
            ITrainerAutoAssignService trainerAutoAssign)
        {
            _repo = repo;
            _leadSourceRepo = leadSourceRepo;
            _staffRepo = staffRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _trainerAutoAssign = trainerAutoAssign;
        }

        [Permission("Leads", "View")]
        public IActionResult Index(
            string? search,
            string? status,
            int? sourceId,
            int? assignedTo)
        {
            ViewBag.LeadSources = _leadSourceRepo.GetAll(search: null);

            ViewBag.Staffs = _staffRepo
                .GetAll()
                .Where(x => x.IsActive)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.SourceId = sourceId;
            ViewBag.AssignedTo = assignedTo;

            var leads = _repo.GetAll(
                search,
                status,
                sourceId,
                assignedTo);

            return View(leads);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Leads", "Add")]
        public IActionResult Save(Lead model)
        {
            try
            {
                if (!model.AssignedTo.HasValue || model.AssignedTo <= 0)
                {
                    var trainerId = _trainerAutoAssign.FindBestTrainerId(model.InterestedIn);
                    if (trainerId.HasValue)
                        model.AssignedTo = trainerId;
                }

                _repo.Insert(model);

                var trainerName = model.AssignedTo.HasValue
                    ? _trainerAutoAssign.GetTrainerName(model.AssignedTo.Value)
                    : null;

                TempData["Success"] = trainerName != null
                    ? $"Lead Added Successfully. Trainer auto-assigned: {trainerName}"
                    : "Lead Added Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Leads", "Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id)
        {
            ViewBag.LeadSources =
                _leadSourceRepo.GetAll(search: null);

            ViewBag.Staffs = _staffRepo
                .GetAll()
                .Where(x => x.IsActive)
                .ToList();

            var model = _repo.GetById(id);

            return PartialView(
                "_EditLead",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Leads", "Edit")]
        public IActionResult Update(Lead model)
        {
            try
            {
                _repo.Update(model);

                TempData["Success"] =
                    "Lead Updated Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }


        [Permission("Leads", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);

                TempData["Success"] =
                    "Lead Deleted Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Leads", "View")]
        public IActionResult Export() => this.ExportCsv("Leads");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Leads", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("Leads", file);
    }
}
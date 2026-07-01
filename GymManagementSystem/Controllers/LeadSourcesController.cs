using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class LeadSourcesController : Controller
    {
        private readonly LeadSourceRepository _repo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public LeadSourcesController(
            LeadSourceRepository repo,
            RolePermissionRepository rolePermissionRepo)
        {
            _repo = repo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        [Permission("LeadSources", "View")]
        public IActionResult Index(string? search)
        {
            var sources = _repo.GetAll(search);

            ViewBag.Search = search;

            return View(sources);
        }

        [HttpPost]
        [Permission("LeadSources", "Add")]
        public IActionResult Save(LeadSourceMaster model)
        {
            try
            {
                _repo.Insert(model);

                TempData["Success"] =
                    "Lead Source Added Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("LeadSources", "Edit")]
        public IActionResult Edit(int id)
        {
            var model = _repo.GetById(id);

            return PartialView(
                "_EditLeadSource",
                model);
        }

        [HttpPost]
        [Permission("LeadSources", "Edit")]
        public IActionResult Update(LeadSourceMaster model)
        {
            try
            {
                _repo.Update(model);

                TempData["Success"] =
                    "Lead Source Updated Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("LeadSources", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);

                TempData["Success"] =
                    "Lead Source Deleted Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("LeadSources", "View")]
        public IActionResult Export() => this.ExportCsv("LeadSources");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("LeadSources", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("LeadSources", file);
    }
}
using GymManagement.Helpers;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class WhatsAppApiSetupController : Controller
    {
        private readonly IWhatsAppApiSetupService _setupService;

        public WhatsAppApiSetupController(IWhatsAppApiSetupService setupService)
        {
            _setupService = setupService;
        }

        [Permission("WhatsAppApiSetup", "View")]
        public IActionResult Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return View(_setupService.GetForm(baseUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("WhatsAppApiSetup", "Edit")]
        public IActionResult Index(WhatsAppApiSetupViewModel model)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (model.HasExistingToken && string.IsNullOrWhiteSpace(model.AccessToken))
                ModelState.Remove(nameof(model.AccessToken));

            if (!ModelState.IsValid)
            {
                model.WebhookUrl = $"{baseUrl}/api/whatsapp/webhook";
                model.FormUrl = $"{baseUrl}/join";
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var result = _setupService.Save(model, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.WebhookUrl = $"{baseUrl}/api/whatsapp/webhook";
                model.FormUrl = $"{baseUrl}/join";
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("WhatsAppApiSetup", "Edit")]
        public async Task<IActionResult> TestConnection([FromBody] WhatsAppApiSetupViewModel? model)
        {
            var result = await _setupService.TestConnectionAsync(model);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}

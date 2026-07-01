using GymManagement.Helpers;
using GymManagement.Services.WhatsApp;
using Microsoft.AspNetCore.Mvc;
namespace GymManagement.Controllers
{
    public class WhatsAppBotController : Controller
    {
        private readonly IWhatsAppBotService _botService;
        private readonly IWhatsAppSettingsProvider _settingsProvider;

        public WhatsAppBotController(
            IWhatsAppBotService botService,
            IWhatsAppSettingsProvider settingsProvider)
        {
            _botService = botService;
            _settingsProvider = settingsProvider;
        }

        [Permission("Leads", "View")]
        public IActionResult Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            ViewBag.FormUrl = _botService.BuildPublicFormUrl(baseUrl);
            ViewBag.WebhookUrl = $"{baseUrl}/api/whatsapp/webhook";
            ViewBag.WhatsAppShareUrl = _botService.BuildWhatsAppDeepLink(
                "Join CloudMex Gym! Fill this form to get started: " + _botService.BuildPublicFormUrl(baseUrl));
            ViewBag.Settings = _settingsProvider.GetSettings();
            ViewBag.IsConfigured = _settingsProvider.IsConfigured;
            return View();
        }
    }
}

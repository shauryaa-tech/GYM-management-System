using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.Services;
using GymManagement.Services.WhatsApp;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    [AllowAnonymous]
    [Route("join")]
    public class PublicJoinController : Controller
    {
        private readonly LeadRepository _leadRepo;
        private readonly WhatsAppBotSessionRepository _sessionRepo;
        private readonly IWhatsAppBotService _botService;
        private readonly ITrainerAutoAssignService _trainerAutoAssign;

        public PublicJoinController(
            LeadRepository leadRepo,
            WhatsAppBotSessionRepository sessionRepo,
            IWhatsAppBotService botService,
            ITrainerAutoAssignService trainerAutoAssign)
        {
            _leadRepo = leadRepo;
            _sessionRepo = sessionRepo;
            _botService = botService;
            _trainerAutoAssign = trainerAutoAssign;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(new PublicLeadFormViewModel());
        }

        [HttpPost("")]
        [HttpPost("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PublicLeadFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sourceId = _sessionRepo.GetLeadSourceId("Public Form")
                ?? _sessionRepo.GetLeadSourceId("WhatsApp Bot")
                ?? 1;

            var lead = new Lead
            {
                LeadName = model.LeadName.Trim(),
                MobileNo = model.MobileNo.Trim(),
                Email = model.Email,
                Gender = model.Gender,
                InterestedIn = model.InterestedIn,
                LeadSourceId = sourceId,
                Status = "New",
                Remarks = "Created via public join form",
                IsConverted = false,
                IsActive = true
            };

            var leadId = _leadRepo.Insert(lead);
            _trainerAutoAssign.AssignTrainerToLead(leadId, model.InterestedIn);
            await _botService.StartSessionAsync(leadId, model.MobileNo, model.LeadName);

            var whatsAppLink = _botService.BuildWhatsAppDeepLink($"Hi CloudMex, I submitted join form. Lead #{leadId}");
            return RedirectToAction(nameof(Success), new { leadId, whatsApp = whatsAppLink });
        }

        [HttpGet("success")]
        public IActionResult Success(int leadId, string? whatsApp, bool paid = false)
        {
            ViewBag.LeadId = leadId;
            ViewBag.WhatsAppLink = whatsApp;
            ViewBag.Paid = paid;
            ViewBag.PaymentToken = _sessionRepo.GetByLeadId(leadId)?.PaymentToken;
            return View();
        }
    }
}

using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Services;
namespace GymManagement.Services.WhatsApp
{
    public class WhatsAppBotService : IWhatsAppBotService
    {
        private readonly WhatsAppBotSessionRepository _sessionRepo;
        private readonly StaffRepository _staffRepo;
        private readonly ClassRepository _classRepo;
        private readonly MembershipRepository _membershipRepo;
        private readonly LeadRepository _leadRepo;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWhatsAppSettingsProvider _settingsProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITrainerAutoAssignService _trainerAutoAssign;

        public WhatsAppBotService(
            WhatsAppBotSessionRepository sessionRepo,
            StaffRepository staffRepo,
            ClassRepository classRepo,
            MembershipRepository membershipRepo,
            LeadRepository leadRepo,
            IWhatsAppService whatsAppService,
            IWhatsAppSettingsProvider settingsProvider,
            IHttpContextAccessor httpContextAccessor,
            ITrainerAutoAssignService trainerAutoAssign)
        {
            _sessionRepo = sessionRepo;
            _staffRepo = staffRepo;
            _classRepo = classRepo;
            _membershipRepo = membershipRepo;
            _leadRepo = leadRepo;
            _whatsAppService = whatsAppService;
            _settingsProvider = settingsProvider;
            _httpContextAccessor = httpContextAccessor;
            _trainerAutoAssign = trainerAutoAssign;
        }

        public string BuildPublicFormUrl(string? baseUrl)
        {
            baseUrl ??= GetBaseUrl();
            return $"{baseUrl}/join";
        }

        public string BuildPaymentUrl(string paymentToken, string? baseUrl)
        {
            baseUrl ??= GetBaseUrl();
            return $"{baseUrl}/Pay/Lead?token={paymentToken}";
        }

        public string BuildWhatsAppDeepLink(string message)
        {
            var phone = new string(_settingsProvider.GetSettings().BusinessPhone.Where(char.IsDigit).ToArray());
            if (phone.Length == 10)
                phone = "91" + phone;
            return $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";
        }

        public async Task StartSessionAsync(int leadId, string phoneNumber, string leadName)
        {
            _sessionRepo.CreateSession(leadId, phoneNumber);
            var session = _sessionRepo.GetByLeadId(leadId);
            if (session == null)
                return;

            var lead = _leadRepo.GetById(leadId);
            var trainerId = lead.AssignedTo ?? _trainerAutoAssign.AssignTrainerToLead(leadId, lead.InterestedIn);

            if (trainerId.HasValue)
            {
                session.SelectedTrainerId = trainerId;
                session.CurrentStep = WhatsAppBotSteps.SelectClass;
                _sessionRepo.Update(session);

                var trainerName = _trainerAutoAssign.GetTrainerName(trainerId.Value) ?? "your trainer";
                var interestText = string.IsNullOrWhiteSpace(lead.InterestedIn) ? "your goal" : lead.InterestedIn;

                await _whatsAppService.SendTextAsync(
                    session.PhoneNumber,
                    $"Hi {leadName}! Based on your interest in *{interestText}*, we assigned trainer *{trainerName}*.\n\nStep 2: Choose your class.");

                await SendClassMenuAsync(session);
                return;
            }

            await SendTrainerMenuAsync(session, leadName);
        }

        public async Task HandleIncomingMessageAsync(string phoneNumber, string? text, string? interactiveId)
        {
            var session = _sessionRepo.GetActiveByPhone(phoneNumber);
            if (session == null)
            {
                await _whatsAppService.SendTextAsync(
                    phoneNumber,
                    "Welcome to CloudMex Gym!\n\nPlease fill our join form first:\n" + BuildPublicFormUrl(null) +
                    "\n\nAfter submitting, continue here to select trainer, class and membership plan.");
                return;
            }

            var input = (interactiveId ?? text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(input))
                return;

            if (input.Equals("pay", StringComparison.OrdinalIgnoreCase))
            {
                await SendPaymentLinkAsync(session);
                return;
            }

            if (input.Equals("menu", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("start", StringComparison.OrdinalIgnoreCase))
            {
                session.CurrentStep = WhatsAppBotSteps.SelectTrainer;
                session.SelectedTrainerId = null;
                session.SelectedClassId = null;
                session.SelectedPlanId = null;
                _sessionRepo.Update(session);
                await SendTrainerMenuAsync(session, session.LeadName ?? "Member");
                return;
            }

            switch (session.CurrentStep)
            {
                case WhatsAppBotSteps.SelectTrainer:
                    await HandleTrainerSelectionAsync(session, input);
                    break;
                case WhatsAppBotSteps.SelectClass:
                    await HandleClassSelectionAsync(session, input);
                    break;
                case WhatsAppBotSteps.SelectPlan:
                    await HandlePlanSelectionAsync(session, input);
                    break;
                case WhatsAppBotSteps.ReadyToPay:
                    await SendPaymentLinkAsync(session);
                    break;
                default:
                    await _whatsAppService.SendTextAsync(
                        phoneNumber,
                        "Your enrollment is complete. Reply *menu* to start again.");
                    break;
            }
        }

        private async Task HandleTrainerSelectionAsync(WhatsAppBotSession session, string input)
        {
            var trainers = _staffRepo.GetTrainers();
            var trainer = ResolveSelection(trainers, input, t => t.StaffId, t => t.StaffName);
            if (trainer == null)
            {
                await _whatsAppService.SendTextAsync(session.PhoneNumber, "Invalid trainer. Please choose from the list or reply *menu*.");
                return;
            }

            session.SelectedTrainerId = trainer.StaffId;
            session.CurrentStep = WhatsAppBotSteps.SelectClass;
            _sessionRepo.Update(session);

            var lead = _leadRepo.GetById(session.LeadId);
            if (lead.LeadId > 0)
            {
                lead.AssignedTo = trainer.StaffId;
                lead.InterestedIn = lead.InterestedIn ?? "WhatsApp Bot Enrollment";
                _leadRepo.Update(lead);
            }

            await _whatsAppService.SendTextAsync(
                session.PhoneNumber,
                $"Trainer selected: *{trainer.StaffName}*\n\nNow choose your class.");

            await SendClassMenuAsync(session);
        }

        private async Task HandleClassSelectionAsync(WhatsAppBotSession session, string input)
        {
            var classes = _classRepo.GetAll(null, session.SelectedTrainerId?.ToString())
                .Where(c => c.IsActive)
                .ToList();

            if (!classes.Any())
                classes = _classRepo.GetAll(null, null).Where(c => c.IsActive).ToList();

            var selected = ResolveSelection(classes, input, c => c.ClassId, c => c.ClassName);
            if (selected == null)
            {
                await _whatsAppService.SendTextAsync(session.PhoneNumber, "Invalid class. Please choose from the list or reply *menu*.");
                return;
            }

            session.SelectedClassId = selected.ClassId;
            session.CurrentStep = WhatsAppBotSteps.SelectPlan;
            _sessionRepo.Update(session);

            await _whatsAppService.SendTextAsync(
                session.PhoneNumber,
                $"Class selected: *{selected.ClassName}*\n\nNow choose your membership plan.");

            await SendPlanMenuAsync(session);
        }

        private async Task HandlePlanSelectionAsync(WhatsAppBotSession session, string input)
        {
            var plans = _membershipRepo.GetAll(null, true);
            var plan = ResolveSelection(plans, input, p => p.PlanId, p => p.PlanName);
            if (plan == null)
            {
                await _whatsAppService.SendTextAsync(session.PhoneNumber, "Invalid plan. Please choose from the list or reply *menu*.");
                return;
            }

            session.SelectedPlanId = plan.PlanId;
            session.CurrentStep = WhatsAppBotSteps.ReadyToPay;
            if (string.IsNullOrWhiteSpace(session.PaymentToken))
                session.PaymentToken = Guid.NewGuid().ToString("N");
            _sessionRepo.Update(session);

            var payUrl = BuildPaymentUrl(session.PaymentToken, null);
            await _whatsAppService.SendTextAsync(
                session.PhoneNumber,
                $"Plan selected: *{plan.PlanName}*\nAmount: *₹{plan.Amount:N0}*\n\n" +
                $"Click below to pay now:\n{payUrl}\n\nOr reply *pay* to get the link again.");
        }

        private async Task SendPaymentLinkAsync(WhatsAppBotSession session)
        {
            if (session.SelectedPlanId == null)
            {
                session.CurrentStep = WhatsAppBotSteps.SelectPlan;
                _sessionRepo.Update(session);
                await SendPlanMenuAsync(session);
                return;
            }

            if (string.IsNullOrWhiteSpace(session.PaymentToken))
                session.PaymentToken = Guid.NewGuid().ToString("N");

            _sessionRepo.Update(session);
            var payUrl = BuildPaymentUrl(session.PaymentToken, null);
            await _whatsAppService.SendTextAsync(session.PhoneNumber, $"Pay Now link:\n{payUrl}");
        }

        private async Task SendTrainerMenuAsync(WhatsAppBotSession session, string leadName)
        {
            var trainers = _staffRepo.GetTrainers();
            await _whatsAppService.SendTextAsync(
                session.PhoneNumber,
                $"Hi {leadName}! Welcome to CloudMex Gym.\n\nStep 1: Choose your trainer.");

            await _whatsAppService.SendListAsync(
                session.PhoneNumber,
                "Select a trainer from the list below.",
                "Choose Trainer",
                trainers.Select(t => ($"trainer_{t.StaffId}", t.StaffName ?? "Trainer", "Personal Trainer")));
        }

        private async Task SendClassMenuAsync(WhatsAppBotSession session)
        {
            var classes = _classRepo.GetAll(null, session.SelectedTrainerId?.ToString())
                .Where(c => c.IsActive)
                .ToList();

            if (!classes.Any())
                classes = _classRepo.GetAll(null, null).Where(c => c.IsActive).ToList();

            await _whatsAppService.SendListAsync(
                session.PhoneNumber,
                "Select a class from the list below.",
                "Choose Class",
                classes.Select(c => (
                    $"class_{c.ClassId}",
                    c.ClassName ?? "Class",
                    $"{c.Schedule ?? "Flexible"} · ₹{c.Amount:N0}")));
        }

        private async Task SendPlanMenuAsync(WhatsAppBotSession session)
        {
            var plans = _membershipRepo.GetAll(null, true);
            await _whatsAppService.SendListAsync(
                session.PhoneNumber,
                "Select a membership plan.",
                "Choose Plan",
                plans.Select(p => (
                    $"plan_{p.PlanId}",
                    p.PlanName ?? "Plan",
                    $"{p.DurationMonths} months · ₹{p.Amount:N0}")));
        }

        private static T? ResolveSelection<T>(List<T> items, string input, Func<T, int> idSelector, Func<T, string?> nameSelector)
        {
            if (input.StartsWith("trainer_", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("class_", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("plan_", StringComparison.OrdinalIgnoreCase))
            {
                var idPart = input.Split('_').LastOrDefault();
                if (int.TryParse(idPart, out var parsedId))
                    return items.FirstOrDefault(x => idSelector(x) == parsedId);
            }

            if (int.TryParse(input, out var index) && index >= 1 && index <= items.Count)
                return items[index - 1];

            return items.FirstOrDefault(x =>
                string.Equals(nameSelector(x), input, StringComparison.OrdinalIgnoreCase));
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request == null
                ? "https://localhost:5052"
                : $"{request.Scheme}://{request.Host}";
        }
    }
}

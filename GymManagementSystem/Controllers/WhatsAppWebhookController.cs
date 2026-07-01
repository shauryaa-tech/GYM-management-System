using System.Text.Json;
using GymManagement.Helpers;
using GymManagement.Services.WhatsApp;
using Microsoft.AspNetCore.Mvc;
namespace GymManagement.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly IWhatsAppBotService _botService;
        private readonly IWhatsAppSettingsProvider _settingsProvider;
        private readonly ILogger<WhatsAppWebhookController> _logger;

        public WhatsAppWebhookController(
            IWhatsAppBotService botService,
            IWhatsAppSettingsProvider settingsProvider,
            ILogger<WhatsAppWebhookController> logger)
        {
            _botService = botService;
            _settingsProvider = settingsProvider;
            _logger = logger;
        }

        [HttpGet("webhook")]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.verify_token")] string? token,
            [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            if (mode == "subscribe" && token == _settingsProvider.GetSettings().VerifyToken)
                return Content(challenge ?? string.Empty);

            return Unauthorized();
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Receive()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("entry", out var entries))
                    return Ok();

                foreach (var entry in entries.EnumerateArray())
                {
                    if (!entry.TryGetProperty("changes", out var changes))
                        continue;

                    foreach (var change in changes.EnumerateArray())
                    {
                        if (!change.TryGetProperty("value", out var value))
                            continue;

                        if (!value.TryGetProperty("messages", out var messages))
                            continue;

                        foreach (var message in messages.EnumerateArray())
                        {
                            var phone = message.GetProperty("from").GetString() ?? string.Empty;
                            string? text = null;
                            string? interactiveId = null;

                            if (message.TryGetProperty("text", out var textObj))
                                text = textObj.GetProperty("body").GetString();

                            if (message.TryGetProperty("interactive", out var interactive))
                            {
                                if (interactive.TryGetProperty("list_reply", out var listReply))
                                    interactiveId = listReply.GetProperty("id").GetString();
                                else if (interactive.TryGetProperty("button_reply", out var buttonReply))
                                    interactiveId = buttonReply.GetProperty("id").GetString();
                            }

                            if (string.Equals(text, "pay", StringComparison.OrdinalIgnoreCase))
                                text = "pay";

                            await _botService.HandleIncomingMessageAsync(phone, text, interactiveId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp webhook parse error");
            }

            return Ok();
        }
    }
}

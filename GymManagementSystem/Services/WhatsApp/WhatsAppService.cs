using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace GymManagement.Services.WhatsApp
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IWhatsAppSettingsProvider _settingsProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            IWhatsAppSettingsProvider settingsProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<WhatsAppService> logger)
        {
            _settingsProvider = settingsProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public bool IsConfigured => _settingsProvider.IsConfigured;

        public async Task SendTextAsync(string phoneNumber, string message)
        {
            if (!IsConfigured)
                return;

            var payload = new
            {
                messaging_product = "whatsapp",
                to = NormalizeTo(phoneNumber),
                type = "text",
                text = new { body = message }
            };

            await PostMessageAsync(payload);
        }

        public async Task SendListAsync(
            string phoneNumber,
            string bodyText,
            string buttonText,
            IEnumerable<(string Id, string Title, string Description)> rows)
        {
            if (!IsConfigured)
                return;

            var rowList = rows.Take(10).Select(r => new
            {
                id = r.Id,
                title = Truncate(r.Title, 24),
                description = Truncate(r.Description, 72)
            }).ToList();

            if (!rowList.Any())
            {
                await SendTextAsync(phoneNumber, bodyText + "\n\nNo options available right now.");
                return;
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                to = NormalizeTo(phoneNumber),
                type = "interactive",
                interactive = new
                {
                    type = "list",
                    body = new { text = Truncate(bodyText, 1024) },
                    action = new
                    {
                        button = Truncate(buttonText, 20),
                        sections = new[]
                        {
                            new
                            {
                                title = "Options",
                                rows = rowList
                            }
                        }
                    }
                }
            };

            await PostMessageAsync(payload);
        }

        private async Task PostMessageAsync(object payload)
        {
            var settings = _settingsProvider.GetSettings();
            try
            {
                var client = _httpClientFactory.CreateClient("WhatsApp");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.AccessToken);

                var baseUrl = (settings.ApiBaseUrl ?? "https://graph.facebook.com").TrimEnd('/');
                var url = $"{baseUrl}/{settings.GraphApiVersion}/{settings.PhoneNumberId}/messages";
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "WhatsApp send failed ({Provider}): {Status} {Body}",
                        settings.ApiProvider,
                        response.StatusCode,
                        body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp send error ({Provider})", settings.ApiProvider);
            }
        }

        private static string NormalizeTo(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 10)
                return "91" + digits;
            return digits;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= max ? value : value[..max];
        }
    }
}

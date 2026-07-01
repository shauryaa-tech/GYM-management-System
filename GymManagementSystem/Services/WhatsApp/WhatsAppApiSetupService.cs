using System.Net.Http.Headers;
using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;

namespace GymManagement.Services.WhatsApp
{
    public class WhatsAppApiSetupService : IWhatsAppApiSetupService
    {
        private readonly WhatsAppApiSetupRepository _repo;
        private readonly IEncryptionService _encryption;
        private readonly IWhatsAppSettingsProvider _settingsProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public WhatsAppApiSetupService(
            WhatsAppApiSetupRepository repo,
            IEncryptionService encryption,
            IWhatsAppSettingsProvider settingsProvider,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _repo = repo;
            _encryption = encryption;
            _settingsProvider = settingsProvider;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public WhatsAppApiSetupViewModel GetForm(string baseUrl)
        {
            var entity = _repo.Get();
            var settings = _settingsProvider.GetSettings();
            var defaultBaseUrl = _config["WhatsApp:SmartPing:ApiBaseUrl"]
                ?? _config["WhatsApp:ApiBaseUrl"]
                ?? "https://graph.facebook.com";

            if (entity == null)
            {
                return new WhatsAppApiSetupViewModel
                {
                    ApiProvider = WhatsAppProviders.SmartPing,
                    ApiBaseUrl = defaultBaseUrl,
                    GraphApiVersion = settings.GraphApiVersion,
                    VerifyToken = string.IsNullOrWhiteSpace(settings.VerifyToken)
                        ? "cloudmex-verify-token"
                        : settings.VerifyToken,
                    WelcomeMessage = "Welcome to CloudMex Gym!",
                    WebhookUrl = $"{baseUrl}/api/whatsapp/webhook",
                    FormUrl = $"{baseUrl}/join",
                    IsConfigured = _settingsProvider.IsConfigured
                };
            }

            return new WhatsAppApiSetupViewModel
            {
                ApiProvider = string.IsNullOrWhiteSpace(entity.ApiProvider)
                    ? WhatsAppProviders.SmartPing
                    : entity.ApiProvider,
                IsEnabled = entity.IsEnabled,
                ApiBaseUrl = string.IsNullOrWhiteSpace(entity.ApiBaseUrl) ? defaultBaseUrl : entity.ApiBaseUrl,
                PhoneNumberId = entity.PhoneNumberId,
                BusinessPhone = entity.BusinessPhone,
                WabaId = entity.WabaId,
                AppId = entity.AppId,
                VerifyToken = entity.VerifyToken,
                GraphApiVersion = entity.GraphApiVersion ?? "v21.0",
                WelcomeMessage = entity.WelcomeMessage,
                HasExistingToken = !string.IsNullOrWhiteSpace(entity.AccessToken),
                LastModified = entity.ModifiedDate,
                WebhookUrl = $"{baseUrl}/api/whatsapp/webhook",
                FormUrl = $"{baseUrl}/join",
                IsConfigured = _settingsProvider.IsConfigured
            };
        }

        public (bool Success, string Message) Save(WhatsAppApiSetupViewModel model, int? userId)
        {
            var provider = string.IsNullOrWhiteSpace(model.ApiProvider)
                ? WhatsAppProviders.SmartPing
                : model.ApiProvider.Trim();

            if (model.IsEnabled)
            {
                if (string.IsNullOrWhiteSpace(model.PhoneNumberId))
                    return (false, "Phone Number ID is required when WhatsApp is enabled.");
                if (string.IsNullOrWhiteSpace(model.VerifyToken))
                    return (false, "Webhook Verify Token is required.");
                if (!model.HasExistingToken && string.IsNullOrWhiteSpace(model.AccessToken))
                    return (false, "API Token is required.");
            }

            var existing = _repo.Get();
            var token = model.AccessToken;

            if (model.HasExistingToken && string.IsNullOrWhiteSpace(token))
                token = existing?.AccessToken;
            else if (!string.IsNullOrWhiteSpace(token))
                token = _encryption.Encrypt(token);

            var defaultBaseUrl = _config["WhatsApp:SmartPing:ApiBaseUrl"]
                ?? _config["WhatsApp:ApiBaseUrl"]
                ?? "https://graph.facebook.com";

            var entity = new WhatsAppApiSettingsEntity
            {
                IsEnabled = model.IsEnabled,
                ApiProvider = provider,
                ApiBaseUrl = string.IsNullOrWhiteSpace(model.ApiBaseUrl)
                    ? defaultBaseUrl
                    : model.ApiBaseUrl.Trim(),
                PhoneNumberId = model.PhoneNumberId?.Trim(),
                BusinessPhone = model.BusinessPhone?.Trim(),
                WabaId = model.WabaId?.Trim(),
                AppId = model.AppId?.Trim(),
                AccessToken = token,
                VerifyToken = model.VerifyToken?.Trim(),
                GraphApiVersion = string.IsNullOrWhiteSpace(model.GraphApiVersion)
                    ? "v21.0"
                    : model.GraphApiVersion.Trim(),
                WelcomeMessage = model.WelcomeMessage?.Trim()
            };

            _repo.Save(entity, userId);
            return (true, $"{provider} WhatsApp API settings saved successfully.");
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync(WhatsAppApiSetupViewModel? model = null)
        {
            string phoneNumberId;
            string accessToken;
            string graphVersion;
            string apiBaseUrl;
            string provider;

            if (model != null &&
                !string.IsNullOrWhiteSpace(model.PhoneNumberId) &&
                !string.IsNullOrWhiteSpace(model.AccessToken))
            {
                phoneNumberId = model.PhoneNumberId.Trim();
                accessToken = model.AccessToken.Trim();
                graphVersion = string.IsNullOrWhiteSpace(model.GraphApiVersion)
                    ? "v21.0"
                    : model.GraphApiVersion.Trim();
                apiBaseUrl = string.IsNullOrWhiteSpace(model.ApiBaseUrl)
                    ? _config["WhatsApp:SmartPing:ApiBaseUrl"] ?? "https://graph.facebook.com"
                    : model.ApiBaseUrl.Trim();
                provider = string.IsNullOrWhiteSpace(model.ApiProvider)
                    ? WhatsAppProviders.SmartPing
                    : model.ApiProvider;
            }
            else
            {
                var settings = _settingsProvider.GetSettings();
                if (!_settingsProvider.IsConfigured)
                    return (false, "Save Phone Number ID and API Token first, then test.");

                phoneNumberId = settings.PhoneNumberId;
                accessToken = settings.AccessToken;
                graphVersion = settings.GraphApiVersion;
                apiBaseUrl = settings.ApiBaseUrl;
                provider = settings.ApiProvider;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("WhatsApp");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var baseUrl = apiBaseUrl.TrimEnd('/');
                var url = $"{baseUrl}/{graphVersion}/{phoneNumberId}";
                var response = await client.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var label = WhatsAppProviders.IsSmartPing(provider) ? "SmartPing" : provider;
                    return (true, $"Connection successful. {label} WhatsApp API credentials are valid.");
                }

                return (false, $"Connection failed ({(int)response.StatusCode}): {Truncate(body, 200)}");
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        private static string Truncate(string value, int max) =>
            value.Length <= max ? value : value[..max] + "...";
    }
}

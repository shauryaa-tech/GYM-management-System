using GymManagement.Data.Repositories;
using GymManagement.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GymManagement.Services.WhatsApp
{
    public class WhatsAppSettingsProvider : IWhatsAppSettingsProvider
    {
        private readonly WhatsAppApiSetupRepository _repo;
        private readonly IEncryptionService _encryption;
        private readonly WhatsAppSettings _fallback;

        public WhatsAppSettingsProvider(
            WhatsAppApiSetupRepository repo,
            IEncryptionService encryption,
            IOptions<WhatsAppSettings> fallback)
        {
            _repo = repo;
            _encryption = encryption;
            _fallback = fallback.Value;
        }

        public bool IsConfigured
        {
            get
            {
                var s = GetSettings();
                return s.Enabled &&
                       !string.IsNullOrWhiteSpace(s.AccessToken) &&
                       !string.IsNullOrWhiteSpace(s.PhoneNumberId);
            }
        }

        public WhatsAppSettings GetSettings()
        {
            var entity = _repo.Get();
            if (entity == null)
                return _fallback;

            var token = DecryptToken(entity.AccessToken);

            if (string.IsNullOrWhiteSpace(entity.PhoneNumberId) &&
                string.IsNullOrWhiteSpace(token))
            {
                return _fallback;
            }

            var provider = string.IsNullOrWhiteSpace(entity.ApiProvider)
                ? WhatsAppProviders.SmartPing
                : entity.ApiProvider;

            return new WhatsAppSettings
            {
                ApiProvider = provider,
                Enabled = entity.IsEnabled,
                PhoneNumberId = entity.PhoneNumberId ?? string.Empty,
                BusinessPhone = entity.BusinessPhone ?? string.Empty,
                AccessToken = token ?? string.Empty,
                VerifyToken = string.IsNullOrWhiteSpace(entity.VerifyToken)
                    ? _fallback.VerifyToken
                    : entity.VerifyToken!,
                GraphApiVersion = string.IsNullOrWhiteSpace(entity.GraphApiVersion)
                    ? "v21.0"
                    : entity.GraphApiVersion,
                ApiBaseUrl = string.IsNullOrWhiteSpace(entity.ApiBaseUrl)
                    ? _fallback.ApiBaseUrl
                    : entity.ApiBaseUrl!,
                AppName = entity.AppId,
                WabaId = entity.WabaId
            };
        }

        private string DecryptToken(string? stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return string.Empty;

            try
            {
                return _encryption.Decrypt(stored);
            }
            catch
            {
                return stored;
            }
        }
    }
}

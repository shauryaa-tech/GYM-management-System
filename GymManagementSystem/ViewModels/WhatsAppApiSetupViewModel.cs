using System.ComponentModel.DataAnnotations;

namespace GymManagement.ViewModels
{
    public class WhatsAppApiSetupViewModel
    {
        [Display(Name = "API Provider")]
        public string ApiProvider { get; set; } = "SmartPing";

        public bool IsEnabled { get; set; }

        [Display(Name = "API Base URL")]
        [MaxLength(500)]
        public string? ApiBaseUrl { get; set; }

        [Display(Name = "Phone Number ID")]
        [MaxLength(100)]
        public string? PhoneNumberId { get; set; }

        [Display(Name = "Business WhatsApp Number")]
        [MaxLength(20)]
        public string? BusinessPhone { get; set; }

        [Display(Name = "WABA ID")]
        [MaxLength(100)]
        public string? WabaId { get; set; }

        [Display(Name = "App Name")]
        [MaxLength(100)]
        public string? AppId { get; set; }

        [Display(Name = "API Token / Access Token")]
        [DataType(DataType.Password)]
        [MaxLength(1000)]
        public string? AccessToken { get; set; }

        [Display(Name = "Webhook Verify Token")]
        [MaxLength(200)]
        public string? VerifyToken { get; set; }

        [Display(Name = "API Version")]
        [MaxLength(20)]
        public string GraphApiVersion { get; set; } = "v21.0";

        [Display(Name = "Welcome Message")]
        [MaxLength(500)]
        public string? WelcomeMessage { get; set; }

        public bool HasExistingToken { get; set; }
        public DateTime? LastModified { get; set; }
        public string? WebhookUrl { get; set; }
        public string? FormUrl { get; set; }
        public bool IsConfigured { get; set; }

        public bool IsSmartPingProvider =>
            string.IsNullOrWhiteSpace(ApiProvider) ||
            string.Equals(ApiProvider, "SmartPing", StringComparison.OrdinalIgnoreCase);
    }
}

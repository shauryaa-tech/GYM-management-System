using System.ComponentModel.DataAnnotations;

namespace GymManagement.ViewModels
{
    public class PaymentGatewayFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Gateway")]
        public string GatewayName { get; set; } = "Paytm";

        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Key ID")]
        public string MerchantId { get; set; } = string.Empty;

        [MaxLength(200)]
        [Display(Name = "Key Secret")]
        [DataType(DataType.Password)]
        public string MerchantKey { get; set; } = string.Empty;

        [MaxLength(100)]
        public string MID { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ChannelId { get; set; }

        [MaxLength(50)]
        public string Website { get; set; } = string.Empty;

        [MaxLength(50)]
        public string IndustryType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string CallbackUrl { get; set; } = string.Empty;

        public string Environment { get; set; } = "Sandbox";

        [MaxLength(500)]
        public string? SandboxBaseUrl { get; set; }

        [MaxLength(500)]
        public string? ProductionBaseUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; }

        public bool IsValidated { get; set; }

        public string? ValidationMessage { get; set; }

        public DateTime? LastValidatedOn { get; set; }

        public bool HasExistingKey { get; set; }

        public string? ValidationToken { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(GatewayName))
                yield return new ValidationResult("Gateway is required.", [nameof(GatewayName)]);

            if (string.IsNullOrWhiteSpace(MerchantId))
                yield return new ValidationResult("Key ID is required.", [nameof(MerchantId)]);

            if (string.IsNullOrWhiteSpace(Environment))
                yield return new ValidationResult("Environment is required.", [nameof(Environment)]);
        }
    }
}

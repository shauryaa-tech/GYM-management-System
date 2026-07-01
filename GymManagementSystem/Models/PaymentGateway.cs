namespace GymManagement.Models

{

    public class PaymentGateway

    {

        public int Id { get; set; }

        public string GatewayName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? MerchantId { get; set; }

        public string? MerchantKey { get; set; }

        public string? MID { get; set; }

        public string? ChannelId { get; set; }

        public string? Website { get; set; }

        public string? IndustryType { get; set; }

        public string? CallbackUrl { get; set; }

        public string Environment { get; set; } = "Sandbox";

        public string? SandboxBaseUrl { get; set; }

        public string? ProductionBaseUrl { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsValidated { get; set; }

        public string? ValidationMessage { get; set; }

        public DateTime? LastValidatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

    }

}


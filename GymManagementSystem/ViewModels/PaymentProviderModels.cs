namespace GymManagement.ViewModels
{
    public class GatewayProviderConfig
    {
        public string GatewayName { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantKey { get; set; } = string.Empty;
        public string? MID { get; set; }
        public string? ChannelId { get; set; }
        public string? Website { get; set; }
        public string? IndustryType { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
        public string Environment { get; set; } = "Sandbox";
        public string? SandboxBaseUrl { get; set; }
        public string? ProductionBaseUrl { get; set; }

        public string BaseUrl =>
            string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase)
                ? ProductionBaseUrl ?? string.Empty
                : SandboxBaseUrl ?? string.Empty;
    }

    public class PaymentOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string CustomerId { get; set; } = string.Empty;
        public string PaymentFor { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
    }

    public class PaymentProviderValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PaymentInitResult
    {
        public bool Success { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TransactionToken { get; set; }
        public string? RedirectUrl { get; set; }
        public string? RazorPayOrderId { get; set; }
        public string? RazorPayKeyId { get; set; }
        public string? CashfreeSessionId { get; set; }
        public string? MerchantId { get; set; }
        public string? Environment { get; set; }
        public string? RawResponse { get; set; }
    }

    public class PaymentVerificationResult
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? RawResponse { get; set; }
    }
}

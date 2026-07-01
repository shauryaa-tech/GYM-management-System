namespace GymManagement.ViewModels
{
    public class PaytmValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PaytmOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string CustomerId { get; set; } = string.Empty;
        public string PaymentFor { get; set; } = string.Empty;
    }

    public class PaytmOrderResult
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? TransactionToken { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RawResponse { get; set; }
    }

    public class PaytmVerificationResult
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public string? RawResponse { get; set; }
    }

    public class PaytmStatusResult
    {
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public string? RawResponse { get; set; }
    }

    public class PaytmRefundResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RefundId { get; set; }
        public string? RawResponse { get; set; }
    }

    public class PaytmGatewayConfig
    {
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantKey { get; set; } = string.Empty;
        public string MID { get; set; } = string.Empty;
        public string? ChannelId { get; set; }
        public string Website { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string Environment { get; set; } = "Sandbox";
        public string BaseUrl { get; set; } = string.Empty;
    }
}

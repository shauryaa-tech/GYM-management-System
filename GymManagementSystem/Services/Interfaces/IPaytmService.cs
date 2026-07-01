using GymManagement.ViewModels;

namespace GymManagement.Services.Interfaces
{
    public interface IPaytmService
    {
        Task<PaytmValidationResult> ValidateCredentialsAsync(PaytmGatewayConfig config);
        Task<PaytmOrderResult> GenerateOrderAsync(PaytmGatewayConfig config, PaytmOrderRequest request);
        Task<PaytmOrderResult> GenerateTransactionTokenAsync(PaytmGatewayConfig config, PaytmOrderRequest request);
        Task<PaytmVerificationResult> VerifyPaymentAsync(PaytmGatewayConfig config, IDictionary<string, string> callbackData);
        Task<PaytmStatusResult> CheckTransactionStatusAsync(PaytmGatewayConfig config, string orderId);
        Task<PaytmRefundResult> RefundPaymentAsync(PaytmGatewayConfig config, string orderId, string transactionId, decimal amount, string refundId);
    }
}

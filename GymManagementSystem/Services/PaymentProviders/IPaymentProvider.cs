using GymManagement.ViewModels;

namespace GymManagement.Services.PaymentProviders
{
    public interface IPaymentProvider
    {
        string GatewayName { get; }

        Task<PaymentProviderValidationResult> ValidateCredentialsAsync(GatewayProviderConfig config);

        Task<PaymentInitResult> InitiatePaymentAsync(GatewayProviderConfig config, PaymentOrderRequest request);

        Task<PaymentVerificationResult> VerifyPaymentAsync(
            GatewayProviderConfig config,
            IDictionary<string, string> callbackData,
            string? rawBody = null);
    }
}

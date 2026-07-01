using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using GymManagement.Services.PaymentProviders;

namespace GymManagement.Services.PaymentProviders.Implementations
{
    public class PaytmPaymentProvider : IPaymentProvider
    {
        private readonly IPaytmService _paytmService;

        public PaytmPaymentProvider(IPaytmService paytmService)
        {
            _paytmService = paytmService;
        }

        public string GatewayName => PaymentGatewayNames.Paytm;

        public async Task<PaymentProviderValidationResult> ValidateCredentialsAsync(GatewayProviderConfig config)
        {
            var result = await _paytmService.ValidateCredentialsAsync(MapConfig(config));
            return new PaymentProviderValidationResult
            {
                Success = result.Success,
                Message = result.Message
            };
        }

        public async Task<PaymentInitResult> InitiatePaymentAsync(GatewayProviderConfig config, PaymentOrderRequest request)
        {
            var result = await _paytmService.GenerateTransactionTokenAsync(MapConfig(config), new PaytmOrderRequest
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                CustomerId = request.CustomerId,
                PaymentFor = request.PaymentFor
            });

            return new PaymentInitResult
            {
                Success = result.Success,
                Gateway = GatewayName,
                OrderId = result.OrderId,
                Message = result.Message,
                TransactionToken = result.TransactionToken,
                RawResponse = result.RawResponse
            };
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            GatewayProviderConfig config,
            IDictionary<string, string> callbackData,
            string? rawBody = null)
        {
            var result = await _paytmService.VerifyPaymentAsync(MapConfig(config), callbackData);
            decimal.TryParse(GetValue(callbackData, "TXNAMOUNT"), out var amount);

            return new PaymentVerificationResult
            {
                Success = result.Success,
                OrderId = result.OrderId,
                TransactionId = result.TransactionId,
                Status = result.Status,
                ResponseCode = result.ResponseCode,
                ResponseMessage = result.ResponseMessage,
                Amount = amount,
                RawResponse = result.RawResponse
            };
        }

        private static PaytmGatewayConfig MapConfig(GatewayProviderConfig config) =>
            new()
            {
                MerchantId = config.MerchantId,
                MerchantKey = config.MerchantKey,
                MID = config.MID ?? config.MerchantId,
                ChannelId = config.ChannelId,
                Website = config.Website ?? "WEBSTAGING",
                IndustryType = config.IndustryType ?? "Retail",
                CallbackUrl = config.CallbackUrl,
                Environment = config.Environment,
                BaseUrl = config.BaseUrl
            };

        private static string GetValue(IDictionary<string, string> data, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (data.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }
    }
}

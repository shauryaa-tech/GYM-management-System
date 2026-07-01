using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GymManagement.ViewModels;
using GymManagement.Services.PaymentProviders;
using Microsoft.Extensions.Options;

namespace GymManagement.Services.PaymentProviders.Implementations
{
    public class RazorpaySettings
    {
        public const string SectionName = "Razorpay";

        public string SandboxBaseUrl { get; set; } = "https://api.razorpay.com";
        public string ProductionBaseUrl { get; set; } = "https://api.razorpay.com";
    }

    public class RazorpayPaymentProvider : IPaymentProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RazorpaySettings _settings;

        public RazorpayPaymentProvider(IHttpClientFactory httpClientFactory, IOptions<RazorpaySettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public string GatewayName => PaymentGatewayNames.Razorpay;

        public async Task<PaymentProviderValidationResult> ValidateCredentialsAsync(GatewayProviderConfig config)
        {
            var error = ValidateConfig(config);
            if (error != null)
            {
                return new PaymentProviderValidationResult { Success = false, Message = error };
            }

            try
            {
                var client = CreateClient(config);
                var response = await client.GetAsync("/v1/payments?count=1");
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return new PaymentProviderValidationResult
                    {
                        Success = true,
                        Message = "Razorpay credentials verified."
                    };
                }

                return new PaymentProviderValidationResult
                {
                    Success = false,
                    Message = "Invalid Razorpay key id or secret."
                };
            }
            catch (Exception ex)
            {
                return new PaymentProviderValidationResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<PaymentInitResult> InitiatePaymentAsync(GatewayProviderConfig config, PaymentOrderRequest request)
        {
            var error = ValidateConfig(config);
            if (error != null)
            {
                return Fail(request.OrderId, error);
            }

            try
            {
                var client = CreateClient(config);
                var payload = new
                {
                    amount = (int)(request.Amount * 100),
                    currency = request.Currency,
                    receipt = request.OrderId,
                    notes = new
                    {
                        paymentFor = request.PaymentFor,
                        customerId = request.CustomerId
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/v1/orders", content);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Fail(request.OrderId, ExtractMessage(raw) ?? "Razorpay order creation failed.", raw);
                }

                using var doc = JsonDocument.Parse(raw);
                var orderId = doc.RootElement.GetProperty("id").GetString();

                return new PaymentInitResult
                {
                    Success = true,
                    Gateway = GatewayName,
                    OrderId = request.OrderId,
                    RazorPayOrderId = orderId,
                    RazorPayKeyId = config.MerchantId,
                    Message = "Razorpay order created.",
                    RawResponse = raw
                };
            }
            catch (Exception ex)
            {
                return Fail(request.OrderId, ex.Message);
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            GatewayProviderConfig config,
            IDictionary<string, string> callbackData,
            string? rawBody = null)
        {
            var paymentId = GetValue(callbackData, "razorpay_payment_id");
            var orderId = GetValue(callbackData, "razorpay_order_id", "receipt");

            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return new PaymentVerificationResult
                {
                    Success = false,
                    OrderId = orderId,
                    ResponseMessage = "Razorpay payment id missing."
                };
            }

            try
            {
                var client = CreateClient(config);
                var response = await client.GetAsync($"/v1/payments/{paymentId}");
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentVerificationResult
                    {
                        Success = false,
                        OrderId = orderId,
                        ResponseMessage = ExtractMessage(raw) ?? "Unable to verify Razorpay payment.",
                        RawResponse = raw
                    };
                }

                using var doc = JsonDocument.Parse(raw);
                var status = doc.RootElement.GetProperty("status").GetString() ?? string.Empty;
                var amount = doc.RootElement.GetProperty("amount").GetInt32() / 100m;
                var razorpayOrderId = doc.RootElement.GetProperty("order_id").GetString() ?? string.Empty;

                var orderResponse = await client.GetAsync($"/v1/orders/{razorpayOrderId}");
                var orderRaw = await orderResponse.Content.ReadAsStringAsync();
                var ourOrderId = orderId;

                if (orderResponse.IsSuccessStatusCode)
                {
                    using var orderDoc = JsonDocument.Parse(orderRaw);
                    if (orderDoc.RootElement.TryGetProperty("receipt", out var receiptEl))
                        ourOrderId = receiptEl.GetString() ?? ourOrderId;
                }

                return new PaymentVerificationResult
                {
                    Success = string.Equals(status, "captured", StringComparison.OrdinalIgnoreCase),
                    OrderId = ourOrderId,
                    TransactionId = paymentId,
                    Status = status,
                    ResponseMessage = status,
                    Amount = amount,
                    RawResponse = raw
                };
            }
            catch (Exception ex)
            {
                return new PaymentVerificationResult
                {
                    Success = false,
                    OrderId = orderId,
                    ResponseMessage = ex.Message
                };
            }
        }

        private HttpClient CreateClient(GatewayProviderConfig config)
        {
            var client = _httpClientFactory.CreateClient("Razorpay");
            client.BaseAddress = new Uri(ResolveBaseUrl(config).TrimEnd('/') + "/");
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.MerchantId}:{config.MerchantKey}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            return client;
        }

        private string ResolveBaseUrl(GatewayProviderConfig config) =>
            !string.IsNullOrWhiteSpace(config.BaseUrl)
                ? config.BaseUrl
                : string.Equals(config.Environment, "Production", StringComparison.OrdinalIgnoreCase)
                    ? _settings.ProductionBaseUrl
                    : _settings.SandboxBaseUrl;

        private static string? ValidateConfig(GatewayProviderConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.MerchantId))
                return "Razorpay Key ID is required.";
            if (string.IsNullOrWhiteSpace(config.MerchantKey))
                return "Razorpay Key Secret is required.";
            return null;
        }

        private static PaymentInitResult Fail(string orderId, string message, string? raw = null) =>
            new()
            {
                Success = false,
                Gateway = PaymentGatewayNames.Razorpay,
                OrderId = orderId,
                Message = message,
                RawResponse = raw
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

        private static string? ExtractMessage(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("error", out var error) &&
                    error.TryGetProperty("description", out var description))
                {
                    return description.GetString();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}

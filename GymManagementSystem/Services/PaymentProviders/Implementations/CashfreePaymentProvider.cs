using System.Text;
using System.Text.Json;
using GymManagement.ViewModels;
using GymManagement.Services.PaymentProviders;
using Microsoft.Extensions.Options;

namespace GymManagement.Services.PaymentProviders.Implementations
{
    public class CashfreeSettings
    {
        public const string SectionName = "Cashfree";

        public string SandboxBaseUrl { get; set; } = "https://sandbox.cashfree.com/pg";
        public string ProductionBaseUrl { get; set; } = "https://api.cashfree.com/pg";
    }

    public class CashfreePaymentProvider : IPaymentProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CashfreeSettings _settings;

        public CashfreePaymentProvider(IHttpClientFactory httpClientFactory, IOptions<CashfreeSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public string GatewayName => PaymentGatewayNames.Cashfree;

        public async Task<PaymentProviderValidationResult> ValidateCredentialsAsync(GatewayProviderConfig config)
        {
            var error = ValidateConfig(config);
            if (error != null)
            {
                return new PaymentProviderValidationResult { Success = false, Message = error };
            }

            return new PaymentProviderValidationResult
            {
                Success = true,
                Message = "Cashfree credentials format verified."
            };
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
                    order_id = request.OrderId,
                    order_amount = request.Amount,
                    order_currency = request.Currency,
                    customer_details = new
                    {
                        customer_id = request.CustomerId,
                        customer_phone = request.CustomerPhone ?? "9999999999"
                    },
                    order_meta = new
                    {
                        return_url = config.CallbackUrl,
                        notify_url = config.CallbackUrl
                    },
                    order_note = request.PaymentFor
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/orders", content);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Fail(request.OrderId, ExtractMessage(raw) ?? "Cashfree order creation failed.", raw);
                }

                using var doc = JsonDocument.Parse(raw);
                var sessionId = doc.RootElement.TryGetProperty("payment_session_id", out var sessionEl)
                    ? sessionEl.GetString()
                    : null;

                return new PaymentInitResult
                {
                    Success = !string.IsNullOrWhiteSpace(sessionId),
                    Gateway = GatewayName,
                    OrderId = request.OrderId,
                    CashfreeSessionId = sessionId,
                    Message = sessionId == null ? "Cashfree session id missing." : "Cashfree payment initiated.",
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
            var orderId = GetValue(callbackData, "order_id", "orderId");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return new PaymentVerificationResult
                {
                    Success = false,
                    ResponseMessage = "Cashfree order id missing."
                };
            }

            try
            {
                var client = CreateClient(config);
                var response = await client.GetAsync($"/orders/{orderId}");
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentVerificationResult
                    {
                        Success = false,
                        OrderId = orderId,
                        ResponseMessage = ExtractMessage(raw) ?? "Cashfree order verification failed.",
                        RawResponse = raw
                    };
                }

                using var doc = JsonDocument.Parse(raw);
                var status = doc.RootElement.TryGetProperty("order_status", out var statusEl)
                    ? statusEl.GetString() ?? string.Empty
                    : string.Empty;
                var amount = doc.RootElement.TryGetProperty("order_amount", out var amountEl)
                    ? amountEl.GetDecimal()
                    : 0m;

                return new PaymentVerificationResult
                {
                    Success = string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase),
                    OrderId = orderId,
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
            var client = _httpClientFactory.CreateClient("Cashfree");
            client.BaseAddress = new Uri(ResolveBaseUrl(config).TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("x-client-id", config.MerchantId);
            client.DefaultRequestHeaders.Add("x-client-secret", config.MerchantKey);
            client.DefaultRequestHeaders.Add("x-api-version", "2023-08-01");
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
                return "Cashfree App ID is required.";
            if (string.IsNullOrWhiteSpace(config.MerchantKey))
                return "Cashfree Secret Key is required.";
            if (string.IsNullOrWhiteSpace(config.CallbackUrl))
                return "Return / Notify URL is required.";
            return null;
        }

        private static PaymentInitResult Fail(string orderId, string message, string? raw = null) =>
            new()
            {
                Success = false,
                Gateway = PaymentGatewayNames.Cashfree,
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
                if (doc.RootElement.TryGetProperty("message", out var message))
                    return message.GetString();
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}

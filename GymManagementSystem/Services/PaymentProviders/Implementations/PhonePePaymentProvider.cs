using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GymManagement.ViewModels;
using GymManagement.Services.PaymentProviders;
using Microsoft.Extensions.Options;

namespace GymManagement.Services.PaymentProviders.Implementations
{
    public class PhonePeSettings
    {
        public const string SectionName = "PhonePe";

        public string SandboxBaseUrl { get; set; } = "https://api-preprod.phonepe.com/apis/pg-sandbox";
        public string ProductionBaseUrl { get; set; } = "https://api.phonepe.com/apis/hermes";
    }

    public class PhonePePaymentProvider : IPaymentProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PhonePeSettings _settings;

        public PhonePePaymentProvider(IHttpClientFactory httpClientFactory, IOptions<PhonePeSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public string GatewayName => PaymentGatewayNames.PhonePe;

        public Task<PaymentProviderValidationResult> ValidateCredentialsAsync(GatewayProviderConfig config)
        {
            var error = ValidateConfig(config);
            if (error != null)
            {
                return Task.FromResult(new PaymentProviderValidationResult
                {
                    Success = false,
                    Message = error
                });
            }

            return Task.FromResult(new PaymentProviderValidationResult
            {
                Success = true,
                Message = "PhonePe credentials format verified."
            });
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
                var endpoint = "/pg/v1/pay";
                var payload = new
                {
                    merchantId = config.MerchantId,
                    merchantTransactionId = request.OrderId,
                    merchantUserId = request.CustomerId,
                    amount = (long)(request.Amount * 100),
                    redirectUrl = config.CallbackUrl,
                    redirectMode = "POST",
                    callbackUrl = config.CallbackUrl,
                    paymentInstrument = new { type = "PAY_PAGE" }
                };

                var base64Payload = EncodePayload(payload);
                var checksum = BuildChecksum(base64Payload, endpoint, config.MerchantKey, config.ChannelId ?? "1");

                var client = _httpClientFactory.CreateClient("PhonePe");
                var url = $"{ResolveBaseUrl(config).TrimEnd('/')}{endpoint}";
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("X-VERIFY", checksum);
                httpRequest.Headers.Add("X-MERCHANT-ID", config.MerchantId);
                httpRequest.Content = new StringContent(
                    JsonSerializer.Serialize(new { request = base64Payload }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.SendAsync(httpRequest);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Fail(request.OrderId, ExtractMessage(raw) ?? $"PhonePe API returned HTTP {(int)response.StatusCode}.", raw);
                }

                using var doc = JsonDocument.Parse(raw);
                if (!doc.RootElement.TryGetProperty("success", out var successEl) || !successEl.GetBoolean())
                {
                    return Fail(request.OrderId, ExtractMessage(raw) ?? "PhonePe rejected the payment request.", raw);
                }

                var redirectUrl = doc.RootElement
                    .GetProperty("data")
                    .GetProperty("instrumentResponse")
                    .GetProperty("redirectInfo")
                    .GetProperty("url")
                    .GetString();

                return new PaymentInitResult
                {
                    Success = true,
                    Gateway = GatewayName,
                    OrderId = request.OrderId,
                    Message = "PhonePe payment initiated.",
                    RedirectUrl = redirectUrl,
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
            var orderId = GetValue(callbackData, "merchantTransactionId", "transactionId", "ORDERID");
            if (string.IsNullOrWhiteSpace(orderId) && callbackData.TryGetValue("response", out var encoded))
            {
                orderId = DecodeResponseOrderId(encoded);
            }

            if (string.IsNullOrWhiteSpace(orderId))
            {
                return new PaymentVerificationResult
                {
                    Success = false,
                    ResponseMessage = "Order id not found in PhonePe callback."
                };
            }

            try
            {
                var endpoint = $"/pg/v1/status/{config.MerchantId}/{orderId}";
                var checksum = BuildChecksum(string.Empty, endpoint, config.MerchantKey, config.ChannelId ?? "1", includePayload: false);

                var client = _httpClientFactory.CreateClient("PhonePe");
                var url = $"{ResolveBaseUrl(config).TrimEnd('/')}{endpoint}";
                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Add("X-VERIFY", checksum);
                httpRequest.Headers.Add("X-MERCHANT-ID", config.MerchantId);

                var response = await client.SendAsync(httpRequest);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentVerificationResult
                    {
                        Success = false,
                        OrderId = orderId,
                        ResponseMessage = ExtractMessage(raw) ?? "PhonePe status check failed.",
                        RawResponse = raw
                    };
                }

                using var doc = JsonDocument.Parse(raw);
                var success = doc.RootElement.TryGetProperty("success", out var successEl) && successEl.GetBoolean();
                var data = doc.RootElement.GetProperty("data");
                var state = data.TryGetProperty("state", out var stateEl) ? stateEl.GetString() ?? string.Empty : string.Empty;
                var txnId = data.TryGetProperty("transactionId", out var txnEl) ? txnEl.GetString() : null;
                var amount = data.TryGetProperty("amount", out var amountEl) ? amountEl.GetInt64() / 100m : 0m;

                return new PaymentVerificationResult
                {
                    Success = success && string.Equals(state, "COMPLETED", StringComparison.OrdinalIgnoreCase),
                    OrderId = orderId,
                    TransactionId = txnId,
                    Status = state,
                    ResponseMessage = state,
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

        private string? ValidateConfig(GatewayProviderConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.MerchantId))
                return "PhonePe Merchant ID is required.";
            if (string.IsNullOrWhiteSpace(config.MerchantKey))
                return "PhonePe Salt Key is required.";
            if (string.IsNullOrWhiteSpace(config.ChannelId))
                return "PhonePe Salt Index is required in Channel ID field.";
            if (string.IsNullOrWhiteSpace(config.CallbackUrl))
                return "Redirect / Callback URL is required.";
            return null;
        }

        private string ResolveBaseUrl(GatewayProviderConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config.BaseUrl))
                return config.BaseUrl;

            return string.Equals(config.Environment, "Production", StringComparison.OrdinalIgnoreCase)
                ? _settings.ProductionBaseUrl
                : _settings.SandboxBaseUrl;
        }

        private static PaymentInitResult Fail(string orderId, string message, string? raw = null) =>
            new()
            {
                Success = false,
                Gateway = PaymentGatewayNames.PhonePe,
                OrderId = orderId,
                Message = message,
                RawResponse = raw
            };

        private static string EncodePayload(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        private static string BuildChecksum(string base64Payload, string endpoint, string saltKey, string saltIndex, bool includePayload = true)
        {
            var input = includePayload ? base64Payload + endpoint + saltKey : endpoint + saltKey;
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant() + "###" + saltIndex;
        }

        private static string? DecodeResponseOrderId(string encoded)
        {
            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("merchantTransactionId", out var orderEl))
                {
                    return orderEl.GetString();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

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
                if (doc.RootElement.TryGetProperty("code", out var code))
                    return code.GetString();
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}

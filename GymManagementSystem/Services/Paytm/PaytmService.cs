using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using GymManagement.Services.Paytm;
using Microsoft.Extensions.Options;

namespace GymManagement.Services.Paytm
{
    public class PaytmSettings
    {
        public const string SectionName = "Paytm";

        public string SandboxBaseUrl { get; set; } = "https://securegw-stage.paytm.in";
        public string ProductionBaseUrl { get; set; } = "https://securegw.paytm.in";
        public string InitiateTransactionPath { get; set; } = "/theia/api/v1/initiateTransaction";
        public string TransactionStatusPath { get; set; } = "/v3/order/status";
        public string RefundPath { get; set; } = "/refund/apply";
    }

    public class PaytmService : IPaytmService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PaytmSettings _settings;

        public PaytmService(IHttpClientFactory httpClientFactory, IOptions<PaytmSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public async Task<PaytmValidationResult> ValidateCredentialsAsync(PaytmGatewayConfig config)
        {
            var testOrder = new PaytmOrderRequest
            {
                OrderId = "VALIDATE_" + DateTime.UtcNow.Ticks,
                Amount = 1.00m,
                CustomerId = "VALIDATION",
                PaymentFor = "CredentialValidation"
            };

            var result = await GenerateTransactionTokenAsync(config, testOrder);

            return new PaytmValidationResult
            {
                Success = result.Success,
                Message = result.Success
                    ? "Merchant Verified"
                    : string.IsNullOrWhiteSpace(result.Message)
                        ? "Paytm credential validation failed."
                        : result.Message
            };
        }

        public Task<PaytmOrderResult> GenerateOrderAsync(PaytmGatewayConfig config, PaytmOrderRequest request)
        {
            return GenerateTransactionTokenAsync(config, request);
        }

        public async Task<PaytmOrderResult> GenerateTransactionTokenAsync(PaytmGatewayConfig config, PaytmOrderRequest request)
        {
            try
            {
                var baseUrl = ResolveBaseUrl(config);
                var bodyObject = new
                {
                    requestType = "Payment",
                    mid = config.MID,
                    websiteName = config.Website,
                    orderId = request.OrderId,
                    callbackUrl = config.CallbackUrl,
                    txnAmount = new
                    {
                        value = request.Amount.ToString("0.00"),
                        currency = request.Currency
                    },
                    userInfo = new
                    {
                        custId = request.CustomerId
                    }
                };

                var bodyJson = PaytmChecksumHelper.SerializeBody(bodyObject);
                var signature = PaytmChecksumHelper.GenerateSignature(bodyJson, config.MerchantKey);

                var payload = new
                {
                    body = bodyObject,
                    head = new { signature }
                };

                var client = _httpClientFactory.CreateClient("Paytm");
                var url = $"{baseUrl.TrimEnd('/')}{_settings.InitiateTransactionPath}?mid={Uri.EscapeDataString(config.MID)}";
                var response = await client.PostAsJsonAsync(url, payload);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PaytmOrderResult
                    {
                        Success = false,
                        OrderId = request.OrderId,
                        Message = ExtractPaytmMessage(raw) ?? $"Paytm API returned HTTP {(int)response.StatusCode}.",
                        RawResponse = raw
                    };
                }

                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                var resultStatus = root.GetProperty("body").GetProperty("resultInfo").GetProperty("resultStatus").GetString();
                var resultMsg = root.GetProperty("body").GetProperty("resultInfo").GetProperty("resultMsg").GetString();

                if (!string.Equals(resultStatus, "S", StringComparison.OrdinalIgnoreCase))
                {
                    return new PaytmOrderResult
                    {
                        Success = false,
                        OrderId = request.OrderId,
                        Message = resultMsg ?? "Paytm rejected the transaction request.",
                        RawResponse = raw
                    };
                }

                var token = root.GetProperty("body").GetProperty("txnToken").GetString();

                return new PaytmOrderResult
                {
                    Success = true,
                    OrderId = request.OrderId,
                    TransactionToken = token,
                    Message = "Transaction token generated successfully.",
                    RawResponse = raw
                };
            }
            catch (Exception ex)
            {
                return new PaytmOrderResult
                {
                    Success = false,
                    OrderId = request.OrderId,
                    Message = ex.Message
                };
            }
        }

        public Task<PaytmVerificationResult> VerifyPaymentAsync(PaytmGatewayConfig config, IDictionary<string, string> callbackData)
        {
            var orderId = GetValue(callbackData, "ORDERID", "ORDER_ID");
            return CheckTransactionStatusAsync(config, orderId).ContinueWith(t =>
            {
                var status = t.Result;
                return new PaytmVerificationResult
                {
                    Success = status.Success && string.Equals(status.Status, "TXN_SUCCESS", StringComparison.OrdinalIgnoreCase),
                    OrderId = orderId,
                    TransactionId = GetValue(callbackData, "TXNID"),
                    Status = status.Status,
                    ResponseCode = status.ResponseCode,
                    ResponseMessage = status.ResponseMessage,
                    RawResponse = status.RawResponse
                };
            });
        }

        public async Task<PaytmStatusResult> CheckTransactionStatusAsync(PaytmGatewayConfig config, string orderId)
        {
            try
            {
                var baseUrl = ResolveBaseUrl(config);
                var bodyObject = new
                {
                    mid = config.MID,
                    orderId
                };

                var bodyJson = PaytmChecksumHelper.SerializeBody(bodyObject);
                var signature = PaytmChecksumHelper.GenerateSignature(bodyJson, config.MerchantKey);

                var payload = new
                {
                    body = bodyObject,
                    head = new
                    {
                        signature,
                        tokenType = "AES"
                    }
                };

                var client = _httpClientFactory.CreateClient("Paytm");
                var url = $"{baseUrl.TrimEnd('/')}{_settings.TransactionStatusPath}";
                var response = await client.PostAsJsonAsync(url, payload);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PaytmStatusResult
                    {
                        Success = false,
                        ResponseMessage = ExtractPaytmMessage(raw) ?? $"Status API returned HTTP {(int)response.StatusCode}.",
                        RawResponse = raw
                    };
                }

                using var doc = JsonDocument.Parse(raw);
                var body = doc.RootElement.GetProperty("body");
                var resultInfo = body.GetProperty("resultInfo");
                var resultStatus = resultInfo.GetProperty("resultStatus").GetString() ?? string.Empty;
                var resultCode = resultInfo.GetProperty("resultCode").GetString() ?? string.Empty;
                var resultMsg = resultInfo.GetProperty("resultMsg").GetString() ?? string.Empty;
                var txnStatus = body.TryGetProperty("txnStatus", out var txnStatusEl)
                    ? txnStatusEl.GetString() ?? string.Empty
                    : string.Empty;

                return new PaytmStatusResult
                {
                    Success = string.Equals(resultStatus, "S", StringComparison.OrdinalIgnoreCase),
                    Status = txnStatus,
                    ResponseCode = resultCode,
                    ResponseMessage = resultMsg,
                    RawResponse = raw
                };
            }
            catch (Exception ex)
            {
                return new PaytmStatusResult
                {
                    Success = false,
                    ResponseMessage = ex.Message
                };
            }
        }

        public async Task<PaytmRefundResult> RefundPaymentAsync(
            PaytmGatewayConfig config,
            string orderId,
            string transactionId,
            decimal amount,
            string refundId)
        {
            try
            {
                var baseUrl = ResolveBaseUrl(config);
                var bodyObject = new
                {
                    mid = config.MID,
                    orderId,
                    txnId = transactionId,
                    refId = refundId,
                    refundAmount = amount.ToString("0.00")
                };

                var bodyJson = PaytmChecksumHelper.SerializeBody(bodyObject);
                var signature = PaytmChecksumHelper.GenerateSignature(bodyJson, config.MerchantKey);

                var payload = new
                {
                    body = bodyObject,
                    head = new { signature }
                };

                var client = _httpClientFactory.CreateClient("Paytm");
                var url = $"{baseUrl.TrimEnd('/')}{_settings.RefundPath}";
                var response = await client.PostAsJsonAsync(url, payload);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new PaytmRefundResult
                    {
                        Success = false,
                        Message = ExtractPaytmMessage(raw) ?? $"Refund API returned HTTP {(int)response.StatusCode}.",
                        RawResponse = raw
                    };
                }

                using var doc = JsonDocument.Parse(raw);
                var resultStatus = doc.RootElement.GetProperty("body").GetProperty("resultInfo").GetProperty("resultStatus").GetString();
                var resultMsg = doc.RootElement.GetProperty("body").GetProperty("resultInfo").GetProperty("resultMsg").GetString();
                var refundRefId = doc.RootElement.GetProperty("body").TryGetProperty("refId", out var refEl)
                    ? refEl.GetString()
                    : refundId;

                return new PaytmRefundResult
                {
                    Success = string.Equals(resultStatus, "S", StringComparison.OrdinalIgnoreCase),
                    Message = resultMsg ?? "Refund processed.",
                    RefundId = refundRefId,
                    RawResponse = raw
                };
            }
            catch (Exception ex)
            {
                return new PaytmRefundResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private string ResolveBaseUrl(PaytmGatewayConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config.BaseUrl))
                return config.BaseUrl;

            return string.Equals(config.Environment, "Production", StringComparison.OrdinalIgnoreCase)
                ? _settings.ProductionBaseUrl
                : _settings.SandboxBaseUrl;
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

        private static string? ExtractPaytmMessage(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("body", out var body) &&
                    body.TryGetProperty("resultInfo", out var resultInfo) &&
                    resultInfo.TryGetProperty("resultMsg", out var msg))
                {
                    return msg.GetString();
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

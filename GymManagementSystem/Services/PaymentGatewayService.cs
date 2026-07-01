using System.Security.Cryptography;
using System.Text;
using GymManagement.Data.Repositories.Interfaces;
using GymManagement.Models;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using GymManagement.Services.PaymentProviders;
using GymManagement.Services.PaymentProviders.Implementations;
using GymManagement.Services.Paytm;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace GymManagement.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IPaymentGatewayRepository _gatewayRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly PaymentProviderFactory _providerFactory;
        private readonly IEncryptionService _encryptionService;
        private readonly IMemoryCache _cache;
        private readonly PaytmSettings _paytmSettings;
        private readonly PhonePeSettings _phonePeSettings;
        private readonly RazorpaySettings _razorpaySettings;
        private readonly CashfreeSettings _cashfreeSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentGatewayService(
            IPaymentGatewayRepository gatewayRepository,
            IPaymentTransactionRepository transactionRepository,
            PaymentProviderFactory providerFactory,
            IEncryptionService encryptionService,
            IMemoryCache cache,
            IOptions<PaytmSettings> paytmSettings,
            IOptions<PhonePeSettings> phonePeSettings,
            IOptions<RazorpaySettings> razorpaySettings,
            IOptions<CashfreeSettings> cashfreeSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _gatewayRepository = gatewayRepository;
            _transactionRepository = transactionRepository;
            _providerFactory = providerFactory;
            _encryptionService = encryptionService;
            _cache = cache;
            _paytmSettings = paytmSettings.Value;
            _phonePeSettings = phonePeSettings.Value;
            _razorpaySettings = razorpaySettings.Value;
            _cashfreeSettings = cashfreeSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaymentGatewayListViewModel> GetListAsync(string? search, string? environment, string? status)
        {
            var items = await _gatewayRepository.GetAllAsync(search, environment, status);

            return new PaymentGatewayListViewModel
            {
                Search = search,
                EnvironmentFilter = environment,
                StatusFilter = status,
                Gateways = items.Select(x => new PaymentGatewayGridItemViewModel
                {
                    Id = x.Id,
                    GatewayName = x.GatewayName,
                    DisplayName = x.DisplayName,
                    Environment = x.Environment,
                    IsValidated = x.IsValidated,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    LastValidatedOn = x.LastValidatedOn,
                    ValidationMessage = x.ValidationMessage
                }).ToList()
            };
        }

        public async Task<PaymentGatewayFormViewModel?> GetFormAsync(int id)
        {
            var entity = await _gatewayRepository.GetByIdAsync(id);
            if (entity == null)
                return null;

            return MapToForm(entity, hasExistingKey: !string.IsNullOrEmpty(entity.MerchantKey));
        }

        public Task<PaymentGatewayFormViewModel> GetEmptyFormAsync(string? gatewayName = null)
        {
            return Task.FromResult(PaymentGatewayDefaults.CreateEmpty(
                gatewayName ?? PaymentGatewayNames.Paytm,
                _paytmSettings,
                _phonePeSettings,
                _razorpaySettings,
                _cashfreeSettings,
                GetRequestBaseUrl()));
        }

        public void ApplyFormDefaults(PaymentGatewayFormViewModel model, string? requestBaseUrl = null)
        {
            PaymentGatewayDefaults.ApplyDefaults(
                model,
                _paytmSettings,
                _phonePeSettings,
                _razorpaySettings,
                _cashfreeSettings,
                requestBaseUrl ?? GetRequestBaseUrl());
        }

        public async Task<(bool Success, string Message, string? ValidationToken)> ValidateCredentialsAsync(PaymentGatewayFormViewModel model)
        {
            if (!PaymentGatewayNames.IsSupported(model.GatewayName))
                return (false, "Unsupported payment gateway selected.", null);

            ApplyFormDefaults(model);
            model = await ResolveMerchantKeyAsync(model);

            var config = PaymentGatewayDefaults.ToProviderConfig(model);
            var provider = _providerFactory.GetProvider(model.GatewayName);
            var result = await provider.ValidateCredentialsAsync(config);

            if (!result.Success)
                return (false, result.Message, null);

            var token = CreateValidationToken(model);
            _cache.Set(GetValidationCacheKey(token), BuildCredentialFingerprint(model), TimeSpan.FromMinutes(30));

            return (true, result.Message, token);
        }

        public async Task<(bool Success, string Message)> SaveAsync(PaymentGatewayFormViewModel model, int? userId, string? validationToken)
        {
            if (!PaymentGatewayNames.IsSupported(model.GatewayName))
                return (false, "Unsupported payment gateway selected.");

            ApplyFormDefaults(model);
            model = await ResolveMerchantKeyAsync(model);

            if (model.Id == 0 && string.IsNullOrWhiteSpace(model.MerchantKey))
                return (false, "Key Secret is required.");

            if (!await IsValidationTokenValidAsync(model, validationToken))
            {
                var revalidate = await ValidateCredentialsAsync(model);
                if (!revalidate.Success)
                    return (false, revalidate.Message);

                validationToken = revalidate.ValidationToken;
            }

            PaymentGateway entity;
            var isNew = model.Id == 0;

            if (isNew)
            {
                entity = new PaymentGateway
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };
            }
            else
            {
                entity = await _gatewayRepository.GetByIdAsync(model.Id)
                    ?? throw new InvalidOperationException("Payment gateway not found.");
            }

            entity.GatewayName = PaymentGatewayNames.Normalize(model.GatewayName);
            entity.DisplayName = model.DisplayName;
            entity.MerchantId = model.MerchantId;

            if (entity.GatewayName == PaymentGatewayNames.Paytm)
            {
                entity.MID = string.IsNullOrWhiteSpace(model.MID) ? model.MerchantId : model.MID;
                entity.Website = model.Website;
                entity.IndustryType = model.IndustryType;
                entity.ChannelId = model.ChannelId;
            }
            else if (entity.GatewayName == PaymentGatewayNames.PhonePe)
            {
                entity.MID = null;
                entity.Website = null;
                entity.IndustryType = null;
                entity.ChannelId = string.IsNullOrWhiteSpace(model.ChannelId) ? "1" : model.ChannelId;
            }
            else
            {
                entity.MID = null;
                entity.Website = null;
                entity.IndustryType = null;
                entity.ChannelId = null;
            }

            entity.CallbackUrl = model.CallbackUrl;
            entity.Environment = model.Environment;
            entity.SandboxBaseUrl = model.SandboxBaseUrl ?? GetDefaultSandboxUrl(entity.GatewayName);
            entity.ProductionBaseUrl = model.ProductionBaseUrl ?? GetDefaultProductionUrl(entity.GatewayName);
            entity.IsActive = model.IsActive;
            entity.IsValidated = true;
            entity.ValidationMessage = "Merchant Verified";
            entity.LastValidatedOn = DateTime.UtcNow;
            entity.ModifiedBy = userId;
            entity.ModifiedDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.MerchantKey))
                entity.MerchantKey = _encryptionService.Encrypt(model.MerchantKey);
            else if (isNew)
                return (false, "Merchant key / secret is required.");

            if (model.IsDefault)
            {
                await _gatewayRepository.ClearDefaultAsync(entity.Id == 0 ? null : entity.Id);
                entity.IsDefault = true;
            }
            else if (!isNew)
            {
                entity.IsDefault = model.IsDefault;
            }

            if (isNew)
            {
                if (model.IsDefault)
                    await _gatewayRepository.ClearDefaultAsync();

                await _gatewayRepository.AddAsync(entity);
            }
            else
            {
                await _gatewayRepository.UpdateAsync(entity);
            }

            if (validationToken != null)
                _cache.Remove(GetValidationCacheKey(validationToken));

            return (true, isNew ? "Payment gateway added successfully." : "Payment gateway updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var entity = await _gatewayRepository.GetByIdAsync(id);
            if (entity == null)
                return (false, "Payment gateway not found.");

            if (entity.IsDefault)
                return (false, "Cannot delete the default payment gateway. Set another gateway as default first.");

            await _gatewayRepository.DeleteAsync(id);
            return (true, "Payment gateway deleted successfully.");
        }

        public async Task<(bool Success, string Message)> SetActiveAsync(int id, bool isActive)
        {
            var entity = await _gatewayRepository.GetByIdAsync(id);
            if (entity == null)
                return (false, "Payment gateway not found.");

            entity.IsActive = isActive;
            entity.ModifiedDate = DateTime.UtcNow;
            await _gatewayRepository.UpdateAsync(entity);

            return (true, isActive ? "Payment gateway activated." : "Payment gateway deactivated.");
        }

        public async Task<(bool Success, string Message)> SetDefaultAsync(int id)
        {
            var entity = await _gatewayRepository.GetByIdAsync(id);
            if (entity == null)
                return (false, "Payment gateway not found.");

            if (!entity.IsValidated)
                return (false, "Only validated gateways can be set as default.");

            await _gatewayRepository.ClearDefaultAsync(id);
            entity.IsDefault = true;
            entity.IsActive = true;
            entity.ModifiedDate = DateTime.UtcNow;
            await _gatewayRepository.UpdateAsync(entity);

            return (true, "Default payment gateway updated.");
        }

        public Task<PaymentGateway?> GetDefaultGatewayAsync() =>
            _gatewayRepository.GetDefaultActiveAsync();

        public async Task<PaymentInitResult> InitiatePaymentAsync(PaymentOrderRequest request)
        {
            var gateway = await _gatewayRepository.GetDefaultActiveAsync();
            if (gateway == null)
            {
                return new PaymentInitResult
                {
                    Success = false,
                    Message = "No active default payment gateway configured."
                };
            }

            var merchantKey = _encryptionService.Decrypt(gateway.MerchantKey ?? string.Empty);
            var config = PaymentGatewayDefaults.ToProviderConfig(gateway, merchantKey);
            var provider = _providerFactory.GetProvider(gateway.GatewayName);

            var transaction = new PaymentTransaction
            {
                MemberId = int.TryParse(request.CustomerId, out var memberId) ? memberId : null,
                OrderId = request.OrderId,
                Gateway = gateway.GatewayName,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentFor = request.PaymentFor,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);

            var result = await provider.InitiatePaymentAsync(config, request);

            transaction.GatewayResponse = result.RawResponse;
            transaction.ResponseMessage = result.Message;
            transaction.Status = result.Success ? "Initiated" : "Failed";
            await _transactionRepository.UpdateAsync(transaction);

            result.Gateway = gateway.GatewayName;
            result.MerchantId = config.MID ?? config.MerchantId;
            result.Environment = config.Environment;
            return result;
        }

        public async Task<PaymentVerificationResult> ProcessCallbackAsync(
            string gatewayName,
            IDictionary<string, string> callbackData,
            string? rawBody = null)
        {
            var orderId = ExtractOrderId(gatewayName, callbackData);
            PaymentTransaction? transaction = null;

            if (!string.IsNullOrWhiteSpace(orderId))
                transaction = await _transactionRepository.GetByOrderIdAsync(orderId);

            transaction ??= await FindTransactionFromCallback(gatewayName, callbackData);

            var gateway = transaction != null
                ? await _gatewayRepository.GetByGatewayNameAsync(transaction.Gateway)
                : await _gatewayRepository.GetByGatewayNameAsync(gatewayName);

            if (gateway == null)
            {
                return new PaymentVerificationResult
                {
                    Success = false,
                    OrderId = orderId,
                    ResponseMessage = "Payment gateway not found."
                };
            }

            var merchantKey = _encryptionService.Decrypt(gateway.MerchantKey ?? string.Empty);
            var config = PaymentGatewayDefaults.ToProviderConfig(gateway, merchantKey);
            var provider = _providerFactory.GetProvider(gateway.GatewayName);
            var verification = await provider.VerifyPaymentAsync(config, callbackData, rawBody);

            if (transaction == null && !string.IsNullOrWhiteSpace(verification.OrderId))
                transaction = await _transactionRepository.GetByOrderIdAsync(verification.OrderId);

            if (transaction == null)
            {
                return new PaymentVerificationResult
                {
                    Success = false,
                    OrderId = verification.OrderId,
                    ResponseMessage = "Transaction not found."
                };
            }

            transaction.TransactionId = verification.TransactionId;
            transaction.Status = verification.Success ? "Success" : verification.Status;
            transaction.ResponseCode = verification.ResponseCode;
            transaction.ResponseMessage = verification.ResponseMessage;
            transaction.GatewayResponse = verification.RawResponse;
            transaction.PaidOn = verification.Success ? DateTime.UtcNow : null;

            await _transactionRepository.UpdateAsync(transaction);

            verification.OrderId = transaction.OrderId;
            return verification;
        }

        public Task<bool> IsValidationTokenValidAsync(PaymentGatewayFormViewModel model, string? validationToken)
        {
            if (string.IsNullOrWhiteSpace(validationToken))
                return Task.FromResult(false);

            if (!_cache.TryGetValue(GetValidationCacheKey(validationToken), out string? fingerprint))
                return Task.FromResult(false);

            return ResolveMerchantKeyAsync(model).ContinueWith(t =>
                fingerprint == BuildCredentialFingerprint(t.Result));
        }

        public bool IsValidationTokenValid(PaymentGatewayFormViewModel model, string? validationToken) =>
            IsValidationTokenValidAsync(model, validationToken).GetAwaiter().GetResult();

        public string CreateValidationToken(PaymentGatewayFormViewModel model)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(BuildCredentialFingerprint(model) + Guid.NewGuid()));
            return Convert.ToHexString(bytes);
        }

        private string? GetRequestBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return null;

            return $"{request.Scheme}://{request.Host}";
        }

        private string GetDefaultSandboxUrl(string gatewayName) => gatewayName switch
        {
            PaymentGatewayNames.PhonePe => _phonePeSettings.SandboxBaseUrl,
            PaymentGatewayNames.Razorpay => _razorpaySettings.SandboxBaseUrl,
            PaymentGatewayNames.Cashfree => _cashfreeSettings.SandboxBaseUrl,
            _ => _paytmSettings.SandboxBaseUrl
        };

        private string GetDefaultProductionUrl(string gatewayName) => gatewayName switch
        {
            PaymentGatewayNames.PhonePe => _phonePeSettings.ProductionBaseUrl,
            PaymentGatewayNames.Razorpay => _razorpaySettings.ProductionBaseUrl,
            PaymentGatewayNames.Cashfree => _cashfreeSettings.ProductionBaseUrl,
            _ => _paytmSettings.ProductionBaseUrl
        };

        private static string GetValidationCacheKey(string token) => $"pg-validation:{token}";

        private static string ExtractOrderId(string gatewayName, IDictionary<string, string> callbackData)
        {
            return gatewayName switch
            {
                PaymentGatewayNames.Paytm => GetCallbackValue(callbackData, "ORDERID", "ORDER_ID"),
                PaymentGatewayNames.PhonePe => GetCallbackValue(callbackData, "merchantTransactionId", "transactionId"),
                PaymentGatewayNames.Razorpay => GetCallbackValue(callbackData, "razorpay_order_id"),
                PaymentGatewayNames.Cashfree => GetCallbackValue(callbackData, "order_id", "orderId"),
                _ => GetCallbackValue(callbackData, "ORDERID", "order_id", "merchantTransactionId")
            };
        }

        private async Task<PaymentTransaction?> FindTransactionFromCallback(string gatewayName, IDictionary<string, string> callbackData)
        {
            if (gatewayName == PaymentGatewayNames.PhonePe &&
                callbackData.TryGetValue("response", out var encoded) &&
                !string.IsNullOrWhiteSpace(encoded))
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("merchantTransactionId", out var orderEl))
                    {
                        var orderId = orderEl.GetString();
                        if (!string.IsNullOrWhiteSpace(orderId))
                            return await _transactionRepository.GetByOrderIdAsync(orderId);
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static string GetCallbackValue(IDictionary<string, string> data, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (data.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }

        private async Task<PaymentGatewayFormViewModel> ResolveMerchantKeyAsync(PaymentGatewayFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.MerchantKey) || model.Id <= 0)
                return model;

            var existing = await _gatewayRepository.GetByIdAsync(model.Id);
            if (existing?.MerchantKey != null)
                model.MerchantKey = _encryptionService.Decrypt(existing.MerchantKey);

            return model;
        }

        private static string BuildCredentialFingerprint(PaymentGatewayFormViewModel model)
        {
            var gateway = PaymentGatewayNames.Normalize(model.GatewayName);
            var mid = gateway == PaymentGatewayNames.Paytm
                ? model.MerchantId
                : (model.MID ?? string.Empty);
            var channelId = gateway == PaymentGatewayNames.PhonePe
                ? (model.ChannelId ?? "1")
                : (model.ChannelId ?? string.Empty);
            var website = gateway == PaymentGatewayNames.Paytm
                ? (model.Environment == "Production" ? "DEFAULT" : "WEBSTAGING")
                : (model.Website ?? string.Empty);

            return string.Join('|',
                gateway,
                model.MerchantId ?? string.Empty,
                model.MerchantKey ?? string.Empty,
                mid,
                channelId,
                website,
                model.Environment ?? string.Empty,
                model.CallbackUrl ?? string.Empty);
        }

        private PaymentGatewayFormViewModel MapToForm(PaymentGateway entity, bool hasExistingKey)
        {
            return new PaymentGatewayFormViewModel
            {
                Id = entity.Id,
                GatewayName = entity.GatewayName,
                DisplayName = entity.DisplayName,
                MerchantId = entity.MerchantId ?? string.Empty,
                MerchantKey = string.Empty,
                MID = entity.MID ?? string.Empty,
                ChannelId = entity.ChannelId,
                Website = entity.Website ?? string.Empty,
                IndustryType = entity.IndustryType ?? string.Empty,
                CallbackUrl = entity.CallbackUrl ?? string.Empty,
                Environment = entity.Environment,
                SandboxBaseUrl = entity.SandboxBaseUrl ?? GetDefaultSandboxUrl(entity.GatewayName),
                ProductionBaseUrl = entity.ProductionBaseUrl ?? GetDefaultProductionUrl(entity.GatewayName),
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault,
                IsValidated = entity.IsValidated,
                ValidationMessage = entity.ValidationMessage,
                LastValidatedOn = entity.LastValidatedOn,
                HasExistingKey = hasExistingKey
            };
        }
    }
}

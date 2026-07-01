using GymManagement.Models;
using GymManagement.ViewModels;
using GymManagement.Services.PaymentProviders.Implementations;
using GymManagement.Services.Paytm;

namespace GymManagement.Services.PaymentProviders
{
    public static class PaymentGatewayDefaults
    {
        public static PaymentGatewayFormViewModel CreateEmpty(
            string gatewayName,
            PaytmSettings paytm,
            PhonePeSettings phonePe,
            RazorpaySettings razorpay,
            CashfreeSettings cashfree,
            string? requestBaseUrl = null)
        {
            var model = new PaymentGatewayFormViewModel
            {
                GatewayName = PaymentGatewayNames.IsSupported(gatewayName)
                    ? PaymentGatewayNames.Normalize(gatewayName)
                    : PaymentGatewayNames.Paytm,
                Environment = "Sandbox",
                IsActive = true
            };

            ApplyDefaults(model, paytm, phonePe, razorpay, cashfree, requestBaseUrl);
            return model;
        }

        public static void ApplyDefaults(
            PaymentGatewayFormViewModel model,
            PaytmSettings paytm,
            PhonePeSettings phonePe,
            RazorpaySettings razorpay,
            CashfreeSettings cashfree,
            string? requestBaseUrl = null)
        {
            model.GatewayName = PaymentGatewayNames.IsSupported(model.GatewayName)
                ? PaymentGatewayNames.Normalize(model.GatewayName)
                : PaymentGatewayNames.Paytm;

            var callbackPath = GetCallbackPath(model.GatewayName);

            model.DisplayName = model.GatewayName switch
            {
                PaymentGatewayNames.PhonePe => "PhonePe Gateway",
                PaymentGatewayNames.Razorpay => "Razorpay Gateway",
                PaymentGatewayNames.Cashfree => "Cashfree Gateway",
                _ => "Paytm Gateway"
            };

            model.CallbackUrl = BuildAbsoluteUrl(requestBaseUrl, callbackPath);

            model.SandboxBaseUrl = model.GatewayName switch
            {
                PaymentGatewayNames.PhonePe => phonePe.SandboxBaseUrl,
                PaymentGatewayNames.Razorpay => razorpay.SandboxBaseUrl,
                PaymentGatewayNames.Cashfree => cashfree.SandboxBaseUrl,
                _ => paytm.SandboxBaseUrl
            };

            model.ProductionBaseUrl = model.GatewayName switch
            {
                PaymentGatewayNames.PhonePe => phonePe.ProductionBaseUrl,
                PaymentGatewayNames.Razorpay => razorpay.ProductionBaseUrl,
                PaymentGatewayNames.Cashfree => cashfree.ProductionBaseUrl,
                _ => paytm.ProductionBaseUrl
            };

            switch (model.GatewayName)
            {
                case PaymentGatewayNames.Paytm:
                    model.MID = model.MerchantId;
                    model.Website = model.Environment == "Production" ? "DEFAULT" : "WEBSTAGING";
                    model.IndustryType = string.IsNullOrWhiteSpace(model.IndustryType) ? "Retail" : model.IndustryType;
                    break;

                case PaymentGatewayNames.PhonePe:
                    if (string.IsNullOrWhiteSpace(model.ChannelId))
                        model.ChannelId = "1";
                    model.MID = string.Empty;
                    model.Website = string.Empty;
                    model.IndustryType = string.Empty;
                    break;

                default:
                    model.MID = string.Empty;
                    model.Website = string.Empty;
                    model.IndustryType = string.Empty;
                    model.ChannelId = null;
                    break;
            }
        }

        public static string GetCallbackPath(string gatewayName) => PaymentGatewayNames.Normalize(gatewayName) switch
        {
            PaymentGatewayNames.PhonePe => "/OnlinePayment/PhonePeCallback",
            PaymentGatewayNames.Razorpay => "/OnlinePayment/RazorpayCallback",
            PaymentGatewayNames.Cashfree => "/OnlinePayment/CashfreeCallback",
            _ => "/OnlinePayment/PaytmCallback"
        };

        public static string BuildAbsoluteUrl(string? baseUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return path;

            return baseUrl.TrimEnd('/') + path;
        }

        public static GatewayProviderConfig ToProviderConfig(PaymentGatewayFormViewModel model) =>
            new()
            {
                GatewayName = model.GatewayName,
                MerchantId = model.MerchantId,
                MerchantKey = model.MerchantKey,
                MID = string.IsNullOrWhiteSpace(model.MID) ? model.MerchantId : model.MID,
                ChannelId = model.ChannelId,
                Website = model.Website,
                IndustryType = model.IndustryType,
                CallbackUrl = model.CallbackUrl,
                Environment = model.Environment,
                SandboxBaseUrl = model.SandboxBaseUrl,
                ProductionBaseUrl = model.ProductionBaseUrl
            };

        public static GatewayProviderConfig ToProviderConfig(PaymentGateway entity, string merchantKeyPlain) =>
            new()
            {
                GatewayName = entity.GatewayName,
                MerchantId = entity.MerchantId ?? string.Empty,
                MerchantKey = merchantKeyPlain,
                MID = entity.MID ?? entity.MerchantId ?? string.Empty,
                ChannelId = entity.ChannelId,
                Website = entity.Website,
                IndustryType = entity.IndustryType,
                CallbackUrl = entity.CallbackUrl ?? string.Empty,
                Environment = entity.Environment,
                SandboxBaseUrl = entity.SandboxBaseUrl,
                ProductionBaseUrl = entity.ProductionBaseUrl
            };
    }
}

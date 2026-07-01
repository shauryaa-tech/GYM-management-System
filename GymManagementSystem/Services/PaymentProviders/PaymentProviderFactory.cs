namespace GymManagement.Services.PaymentProviders
{
    public class PaymentProviderFactory
    {
        private readonly IReadOnlyDictionary<string, IPaymentProvider> _providers;

        public PaymentProviderFactory(IEnumerable<IPaymentProvider> providers)
        {
            _providers = providers.ToDictionary(
                x => x.GatewayName,
                StringComparer.OrdinalIgnoreCase);
        }

        public IPaymentProvider GetProvider(string gatewayName)
        {
            if (!PaymentGatewayNames.IsSupported(gatewayName))
            {
                throw new InvalidOperationException($"Unsupported payment gateway: {gatewayName}");
            }

            var normalized = PaymentGatewayNames.Normalize(gatewayName);
            return _providers[normalized];
        }

        public bool TryGetProvider(string gatewayName, out IPaymentProvider? provider)
        {
            provider = null;
            if (!PaymentGatewayNames.IsSupported(gatewayName))
                return false;

            return _providers.TryGetValue(PaymentGatewayNames.Normalize(gatewayName), out provider);
        }
    }
}

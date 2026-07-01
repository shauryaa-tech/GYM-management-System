namespace GymManagement.Services.PaymentProviders
{
    public static class PaymentGatewayNames
    {
        public const string Paytm = "Paytm";
        public const string PhonePe = "PhonePe";
        public const string Razorpay = "Razorpay";
        public const string Cashfree = "Cashfree";

        public static readonly string[] All =
        {
            Paytm,
            PhonePe,
            Razorpay,
            Cashfree
        };

        public static bool IsSupported(string? gatewayName) =>
            All.Any(x => string.Equals(x, gatewayName, StringComparison.OrdinalIgnoreCase));

        public static string Normalize(string gatewayName) =>
            All.First(x => string.Equals(x, gatewayName, StringComparison.OrdinalIgnoreCase));
    }
}

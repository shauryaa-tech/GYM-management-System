namespace GymManagement.Services.WhatsApp
{
    public static class WhatsAppProviders
    {
        public const string SmartPing = "SmartPing";
        public const string Meta = "Meta";

        public static bool IsSmartPing(string? provider) =>
            string.IsNullOrWhiteSpace(provider) ||
            string.Equals(provider, SmartPing, StringComparison.OrdinalIgnoreCase);

        public static bool IsMeta(string? provider) =>
            string.Equals(provider, Meta, StringComparison.OrdinalIgnoreCase);
    }
}

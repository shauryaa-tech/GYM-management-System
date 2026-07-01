namespace GymManagement.Services.WhatsApp
{
    public class WhatsAppSettings
    {
        public const string SectionName = "WhatsApp";

        public string ApiProvider { get; set; } = WhatsAppProviders.SmartPing;
        public bool Enabled { get; set; }
        public string PhoneNumberId { get; set; } = string.Empty;
        public string BusinessPhone { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string VerifyToken { get; set; } = string.Empty;
        public string GraphApiVersion { get; set; } = "v21.0";
        public string ApiBaseUrl { get; set; } = "https://graph.facebook.com";
        public string? AppName { get; set; }
        public string? WabaId { get; set; }
    }
}

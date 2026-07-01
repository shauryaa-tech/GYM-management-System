namespace GymManagement.Models
{
    public class WhatsAppApiSettingsEntity
    {
        public int Id { get; set; } = 1;
        public bool IsEnabled { get; set; }
        public string ApiProvider { get; set; } = "SmartPing";
        public string? ApiBaseUrl { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? BusinessPhone { get; set; }
        public string? WabaId { get; set; }
        public string? AppId { get; set; }
        public string? AccessToken { get; set; }
        public string? VerifyToken { get; set; }
        public string GraphApiVersion { get; set; } = "v21.0";
        public string? WelcomeMessage { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

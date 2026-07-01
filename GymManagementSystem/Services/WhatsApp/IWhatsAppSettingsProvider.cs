namespace GymManagement.Services.WhatsApp
{
    public interface IWhatsAppSettingsProvider
    {
        WhatsAppSettings GetSettings();
        bool IsConfigured { get; }
    }
}

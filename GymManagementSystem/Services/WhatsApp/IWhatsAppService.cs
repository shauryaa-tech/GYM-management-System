namespace GymManagement.Services.WhatsApp
{
    public interface IWhatsAppService
    {
        bool IsConfigured { get; }
        Task SendTextAsync(string phoneNumber, string message);
        Task SendListAsync(string phoneNumber, string bodyText, string buttonText, IEnumerable<(string Id, string Title, string Description)> rows);
    }
}

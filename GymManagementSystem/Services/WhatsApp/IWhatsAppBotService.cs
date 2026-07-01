namespace GymManagement.Services.WhatsApp
{
    public interface IWhatsAppBotService
    {
        Task HandleIncomingMessageAsync(string phoneNumber, string? text, string? interactiveId);
        Task StartSessionAsync(int leadId, string phoneNumber, string leadName);
        string BuildPaymentUrl(string paymentToken, string? baseUrl);
        string BuildWhatsAppDeepLink(string message);
        string BuildPublicFormUrl(string? baseUrl);
    }
}

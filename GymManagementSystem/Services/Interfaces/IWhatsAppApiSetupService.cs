using GymManagement.ViewModels;

namespace GymManagement.Services.Interfaces
{
    public interface IWhatsAppApiSetupService
    {
        WhatsAppApiSetupViewModel GetForm(string baseUrl);
        (bool Success, string Message) Save(WhatsAppApiSetupViewModel model, int? userId);
        Task<(bool Success, string Message)> TestConnectionAsync(WhatsAppApiSetupViewModel? model = null);
    }
}

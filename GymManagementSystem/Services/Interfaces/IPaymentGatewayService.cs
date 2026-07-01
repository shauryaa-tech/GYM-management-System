using GymManagement.Models;
using GymManagement.ViewModels;

namespace GymManagement.Services.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<PaymentGatewayListViewModel> GetListAsync(string? search, string? environment, string? status);
        Task<PaymentGatewayFormViewModel?> GetFormAsync(int id);
        Task<PaymentGatewayFormViewModel> GetEmptyFormAsync(string? gatewayName = null);
        void ApplyFormDefaults(PaymentGatewayFormViewModel model, string? requestBaseUrl = null);
        Task<(bool Success, string Message, string? ValidationToken)> ValidateCredentialsAsync(PaymentGatewayFormViewModel model);
        Task<(bool Success, string Message)> SaveAsync(PaymentGatewayFormViewModel model, int? userId, string? validationToken);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<(bool Success, string Message)> SetActiveAsync(int id, bool isActive);
        Task<(bool Success, string Message)> SetDefaultAsync(int id);
        Task<PaymentGateway?> GetDefaultGatewayAsync();
        Task<PaymentInitResult> InitiatePaymentAsync(PaymentOrderRequest request);
        Task<PaymentVerificationResult> ProcessCallbackAsync(string gatewayName, IDictionary<string, string> callbackData, string? rawBody = null);
        bool IsValidationTokenValid(PaymentGatewayFormViewModel model, string? validationToken);
        Task<bool> IsValidationTokenValidAsync(PaymentGatewayFormViewModel model, string? validationToken);
        string CreateValidationToken(PaymentGatewayFormViewModel model);
    }
}

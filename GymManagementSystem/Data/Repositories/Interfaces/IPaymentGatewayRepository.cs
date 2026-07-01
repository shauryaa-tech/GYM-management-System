using GymManagement.Models;



namespace GymManagement.Data.Repositories.Interfaces

{

    public interface IPaymentGatewayRepository

    {

        Task<List<PaymentGateway>> GetAllAsync(string? search, string? environment, string? status);

        Task<PaymentGateway?> GetByIdAsync(int id);

        Task<PaymentGateway?> GetDefaultActiveAsync();

        Task<PaymentGateway?> GetByGatewayNameAsync(string gatewayName);

        Task AddAsync(PaymentGateway gateway);

        Task UpdateAsync(PaymentGateway gateway);

        Task DeleteAsync(int id);

        Task ClearDefaultAsync(int? exceptId = null);

    }



    public interface IPaymentTransactionRepository

    {

        Task<PaymentTransaction?> GetByOrderIdAsync(string orderId);

        Task AddAsync(PaymentTransaction transaction);

        Task UpdateAsync(PaymentTransaction transaction);

    }

}


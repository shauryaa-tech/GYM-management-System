namespace GymManagement.ViewModels
{
    public class PaymentGatewayListViewModel
    {
        public List<PaymentGatewayGridItemViewModel> Gateways { get; set; } = new();
        public string? Search { get; set; }
        public string? EnvironmentFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class PaymentGatewayGridItemViewModel
    {
        public int Id { get; set; }
        public string GatewayName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public bool IsValidated { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastValidatedOn { get; set; }
        public string? ValidationMessage { get; set; }
    }
}

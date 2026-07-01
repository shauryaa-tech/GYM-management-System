namespace GymManagement.Models

{

    public class PaymentTransaction

    {

        public int Id { get; set; }

        public int? MemberId { get; set; }

        public string OrderId { get; set; } = string.Empty;

        public string? TransactionId { get; set; }

        public string Gateway { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        public string? PaymentFor { get; set; }

        public string Status { get; set; } = "Pending";

        public string? ResponseCode { get; set; }

        public string? ResponseMessage { get; set; }

        public string? GatewayResponse { get; set; }

        public DateTime? PaidOn { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    }

}


namespace GymManagement.Models
{
    public class WhatsAppBotSession
    {
        public int SessionId { get; set; }
        public int LeadId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string CurrentStep { get; set; } = WhatsAppBotSteps.SelectTrainer;
        public int? SelectedTrainerId { get; set; }
        public int? SelectedClassId { get; set; }
        public int? SelectedPlanId { get; set; }
        public string? PaymentToken { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string? LeadName { get; set; }
    }

    public static class WhatsAppBotSteps
    {
        public const string SelectTrainer = "SelectTrainer";
        public const string SelectClass = "SelectClass";
        public const string SelectPlan = "SelectPlan";
        public const string ReadyToPay = "ReadyToPay";
        public const string Completed = "Completed";
    }
}

namespace GymManagement.Models
{
    public class EquipmentMaintenance
    {
        public int MaintenanceId { get; set; }
        public int EquipmentId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string MaintenanceType { get; set; } = "";
        public decimal Cost { get; set; }
        public string? PaymentMode { get; set; }
        public string? VendorName { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string Status { get; set; } = "Completed";
        public string? Remarks { get; set; }

        public string? EquipmentName { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
    }
}

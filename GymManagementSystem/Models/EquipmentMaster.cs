namespace GymManagement.Models
{
    public class EquipmentMaster
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = "";
        public string? Category { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? Quantity { get; set; }
        public string? ConditionStatus { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

namespace GymManagement.Models
{
    public class DietMaster
    {
        public int DietId { get; set; }
        public string DietName { get; set; } = "";
        public string Category { get; set; } = "";
        public string MealType { get; set; } = "";
        public decimal? Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

namespace GymManagement.Models
{
    public class ExpenseHeadMaster
    {
        public int ExpenseHeadId { get; set; }
        public string HeadName { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

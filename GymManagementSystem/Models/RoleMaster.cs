namespace GymManagement.Models
{
    public class RoleMaster
    {
        public long RoleId { get; set; }

        public string RoleName { get; set; } = "";

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
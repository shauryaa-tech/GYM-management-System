namespace GymManagement.Models
{
    public class UserMaster
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Email { get; set; } = "";
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? RoleName { get; set; }
    }
}

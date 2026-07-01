namespace GymManagement.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";
        public DateTime? LastLogin { get; set; }
        public string? ProfilePhoto { get; set; }
    }
}
using GymManagement.Models;

namespace GymManagement.ViewModels
{
    public class UsersRolesViewModel
    {
        public List<UserMaster> Users { get; set; } = new();

        public List<RoleMaster> Roles { get; set; } = new();

        // NEW
        public RoleMaster Role { get; set; } = new();

        // NEW
        public List<RolePermission> Permissions { get; set; } = new();
    }
}
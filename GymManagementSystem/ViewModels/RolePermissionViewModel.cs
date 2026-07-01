using GymManagement.Models;

namespace GymManagement.ViewModels
{
    public class RolePermissionViewModel
    {
        public RoleMaster Role { get; set; } = new();

        public List<RolePermission> Permissions { get; set; } = new();
    }
}
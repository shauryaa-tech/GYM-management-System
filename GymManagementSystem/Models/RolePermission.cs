namespace GymManagement.Models
{
    public class RolePermission
    {
        public long RolePermissionId { get; set; }

        public long RoleId { get; set; }

        public int PermissionId { get; set; }

        public bool CanView { get; set; }

        public bool CanAdd { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool CanExport { get; set; }

        public string? ModuleName { get; set; }

        public string? DisplayName { get; set; }
    }
}
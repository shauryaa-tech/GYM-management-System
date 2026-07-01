namespace GymManagement.Models
{
    public class PermissionMaster
    {
        public int PermissionId { get; set; }

        public string ModuleName { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
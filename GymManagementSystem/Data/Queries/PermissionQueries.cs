namespace GymManagement.Data.Queries
{
    public static class PermissionQueries
    {
        public const string GetAll = @"
        SELECT *
        FROM PermissionMaster
        WHERE IsActive=1
        ORDER BY SortOrder";
    }
}
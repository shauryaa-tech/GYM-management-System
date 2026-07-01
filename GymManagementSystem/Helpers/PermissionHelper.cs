using GymManagement.Data.Repositories;
using Microsoft.AspNetCore.Http;

namespace GymManagement.Helpers
{
    public static class PermissionHelper
    {
        public static bool CanView(
            HttpContext context,
            RolePermissionRepository repo,
            string module)
        {
            int? roleId = context.Session.GetInt32("RoleId");

            if (roleId == null)
                return false;

            // Super Admin always has permission
            if (roleId == 1)
                return true;

            return repo.HasPermission(roleId.Value, module, "View");
        }

        public static bool CanAdd(
            HttpContext context,
            RolePermissionRepository repo,
            string module)
        {
            int? roleId = context.Session.GetInt32("RoleId");

            if (roleId == null)
                return false;

            if (roleId == 1)
                return true;

            return repo.HasPermission(roleId.Value, module, "Add");
        }

        public static bool CanEdit(
            HttpContext context,
            RolePermissionRepository repo,
            string module)
        {
            int? roleId = context.Session.GetInt32("RoleId");

            if (roleId == null)
                return false;

            if (roleId == 1)
                return true;

            return repo.HasPermission(roleId.Value, module, "Edit");
        }

        public static bool CanDelete(
            HttpContext context,
            RolePermissionRepository repo,
            string module)
        {
            int? roleId = context.Session.GetInt32("RoleId");

            if (roleId == null)
                return false;

            if (roleId == 1)
                return true;

            return repo.HasPermission(roleId.Value, module, "Delete");
        }

        public static bool CanExport(
            HttpContext context,
            RolePermissionRepository repo,
            string module)
        {
            int? roleId = context.Session.GetInt32("RoleId");

            if (roleId == null)
                return false;

            if (roleId == 1)
                return true;

            return repo.HasPermission(roleId.Value, module, "Export");
        }

        public static bool CanAccessPayroll(
            HttpContext context,
            RolePermissionRepository repo) =>
            CanView(context, repo, "Payroll")
            || CanView(context, repo, "SalaryProcessing")
            || CanView(context, repo, "StaffAttendance")
            || CanView(context, repo, "SalaryRuleMaster");
    }
}
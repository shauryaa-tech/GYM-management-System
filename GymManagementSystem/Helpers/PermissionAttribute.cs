using GymManagement.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GymManagement.Helpers
{
    public class PermissionAttribute : ActionFilterAttribute
    {
        private readonly string _module;
        private readonly string _permission;

        public PermissionAttribute(string module, string permission)
        {
            _module = module;
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var repo = context.HttpContext.RequestServices
                .GetService(typeof(RolePermissionRepository))
                as RolePermissionRepository;

            if (repo == null)
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Home",
                    null);
                return;
            }

            int? roleId = context.HttpContext.Session.GetInt32("RoleId");

            if (roleId == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null);
                return;
            }

            // Super Admin
            if (roleId == 1)
                return;

            if (!repo.HasPermission(roleId.Value, _module, _permission))
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Home",
                    null);
            }
        }
    }
}
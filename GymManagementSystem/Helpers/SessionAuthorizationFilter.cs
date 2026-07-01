using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GymManagement.Helpers
{
    public class SessionAuthorizationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (HasAllowAnonymous(context))
                return;

            if (context.HttpContext.Session.GetInt32("RoleId") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        private static bool HasAllowAnonymous(ActionExecutingContext context)
        {
            var endpoint = context.ActionDescriptor.EndpointMetadata;
            if (endpoint.Any(m => m is AllowAnonymousAttribute))
                return true;

            if (context.Controller.GetType().IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
                return true;

            var actionMethod = (context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.MethodInfo;
            return actionMethod != null &&
                   actionMethod.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
        }
    }
}

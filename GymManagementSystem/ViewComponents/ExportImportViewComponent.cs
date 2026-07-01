using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.ViewComponents
{
    public class ExportImportViewComponent : ViewComponent
    {
        private readonly RolePermissionRepository _permissionRepo;

        public ExportImportViewComponent(RolePermissionRepository permissionRepo)
        {
            _permissionRepo = permissionRepo;
        }

        public IViewComponentResult Invoke(
            string module,
            bool allowImport = false,
            string? exportAction = null,
            string? importAction = null,
            string? redirectAction = null,
            bool exportAsExcel = false,
            Dictionary<string, string>? routeValues = null)
        {
            var controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
            var canExport = PermissionHelper.CanExport(HttpContext, _permissionRepo, module)
                || PermissionHelper.CanView(HttpContext, _permissionRepo, module);
            var canImport = allowImport && PermissionHelper.CanAdd(HttpContext, _permissionRepo, module);

            var model = new ExportImportToolbarViewModel
            {
                Module = module,
                Controller = controller,
                ExportAction = exportAction ?? "Export",
                ImportAction = importAction ?? "Import",
                RedirectAction = redirectAction ?? "Index",
                AllowImport = allowImport,
                QueryString = Request.QueryString.Value ?? "",
                RouteValues = routeValues ?? new Dictionary<string, string>(),
                CanExport = canExport,
                CanImport = canImport,
                ExportAsExcel = exportAsExcel
            };

            return View(model);
        }
    }
}

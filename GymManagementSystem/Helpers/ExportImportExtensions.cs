using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Helpers
{
    public static class ExportImportExtensions
    {
        public static IActionResult ExportCsv(this Controller controller, string module)
        {
            var svc = controller.HttpContext.RequestServices.GetRequiredService<IModuleCsvService>();
            var file = svc.BuildExport(module, controller.Request.Query);
            return controller.File(file.Content, file.ContentType, file.FileName);
        }

        public static IActionResult ImportCsv(this Controller controller, string module, IFormFile? file, string redirectAction = "Index")
            => ImportCsv(controller, module, file, redirectAction, null);

        public static IActionResult ImportCsv(
            this Controller controller,
            string module,
            IFormFile? file,
            string redirectAction,
            object? routeValues)
        {
            if (file == null || file.Length == 0)
            {
                controller.TempData["Error"] = "Please select a CSV file to import.";
                return routeValues == null
                    ? controller.RedirectToAction(redirectAction)
                    : controller.RedirectToAction(redirectAction, routeValues);
            }

            var svc = controller.HttpContext.RequestServices.GetRequiredService<IModuleCsvService>();
            CsvImportResult result;
            using (var stream = file.OpenReadStream())
            {
                result = svc.Import(module, stream);
            }

            controller.TempData[result.FailedCount == 0 && result.SuccessCount > 0 ? "Success" : "Error"] = result.Message;
            return routeValues == null
                ? controller.RedirectToAction(redirectAction)
                : controller.RedirectToAction(redirectAction, routeValues);
        }
    }
}

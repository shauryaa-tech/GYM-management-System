using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class PaymentGatewayController : Controller
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly RolePermissionRepository _permissionRepo;

        public PaymentGatewayController(
            IPaymentGatewayService paymentGatewayService,
            RolePermissionRepository permissionRepo)
        {
            _paymentGatewayService = paymentGatewayService;
            _permissionRepo = permissionRepo;
        }

        [Permission("PaymentGateway", "View")]
        public async Task<IActionResult> Index(string? search, string? environment, string? status)
        {
            var model = await _paymentGatewayService.GetListAsync(search, environment, status);
            return View(model);
        }

        [Permission("PaymentGateway", "Add")]
        public async Task<IActionResult> Create()
        {
            var model = await _paymentGatewayService.GetEmptyFormAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("PaymentGateway", "Add")]
        public async Task<IActionResult> Create(PaymentGatewayFormViewModel model)
        {
            PrepareGatewayForm(model);

            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            var result = await _paymentGatewayService.SaveAsync(model, userId, model.ValidationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [Permission("PaymentGateway", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _paymentGatewayService.GetFormAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("PaymentGateway", "Edit")]
        public async Task<IActionResult> Edit(PaymentGatewayFormViewModel model)
        {
            if (model.HasExistingKey && string.IsNullOrWhiteSpace(model.MerchantKey))
                ModelState.Remove(nameof(model.MerchantKey));

            PrepareGatewayForm(model);

            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            var result = await _paymentGatewayService.SaveAsync(model, userId, model.ValidationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateApi([FromBody] PaymentGatewayFormViewModel model)
        {
            if (!CanValidateGateway())
            {
                return Json(new
                {
                    success = false,
                    message = "You do not have permission to validate payment gateways."
                });
            }

            var result = await _paymentGatewayService.ValidateCredentialsAsync(model);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                validationToken = result.ValidationToken,
                isValidated = result.Success
            });
        }

        [Permission("PaymentGateway", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentGatewayService.DeleteAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [Permission("PaymentGateway", "Edit")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _paymentGatewayService.SetActiveAsync(id, true);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [Permission("PaymentGateway", "Edit")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _paymentGatewayService.SetActiveAsync(id, false);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [Permission("PaymentGateway", "Edit")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var result = await _paymentGatewayService.SetDefaultAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private void PrepareGatewayForm(PaymentGatewayFormViewModel model)
        {
            _paymentGatewayService.ApplyFormDefaults(model);
            ModelState.Clear();
            TryValidateModel(model);

            foreach (var key in new[]
            {
                nameof(PaymentGatewayFormViewModel.MID),
                nameof(PaymentGatewayFormViewModel.Website),
                nameof(PaymentGatewayFormViewModel.IndustryType),
                nameof(PaymentGatewayFormViewModel.ChannelId),
                nameof(PaymentGatewayFormViewModel.CallbackUrl),
                nameof(PaymentGatewayFormViewModel.DisplayName),
                nameof(PaymentGatewayFormViewModel.SandboxBaseUrl),
                nameof(PaymentGatewayFormViewModel.ProductionBaseUrl)
            })
            {
                ModelState.Remove(key);
            }
        }

        private bool CanValidateGateway()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null)
                return false;

            if (roleId == 1)
                return true;

            return _permissionRepo.HasPermission(roleId.Value, "PaymentGateway", "Add")
                || _permissionRepo.HasPermission(roleId.Value, "PaymentGateway", "Edit");
        }
    }
}

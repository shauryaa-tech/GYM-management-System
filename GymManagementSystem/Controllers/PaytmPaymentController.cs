using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Services.Interfaces;
using GymManagement.Services.PaymentProviders;
using GymManagement.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    [AllowAnonymous]
    public class PaytmPaymentController : Controller
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly PaymentRepository _paymentRepository;

        public PaytmPaymentController(
            IPaymentGatewayService paymentGatewayService,
            PaymentRepository paymentRepository)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentRepository = paymentRepository;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback()
        {
            var callbackData = Request.Form
                .ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            var result = await _paymentGatewayService.ProcessCallbackAsync(PaymentGatewayNames.Paytm, callbackData);

            if (result.Success)
            {
                var memberId = HttpContext.Session.GetInt32("PendingPaymentMemberId");
                var remarks = HttpContext.Session.GetString("PendingPaymentRemarks")
                    ?? HttpContext.Session.GetString("PendingPaymentFor")
                    ?? "Online Payment";

                if (memberId.HasValue)
                {
                    _paymentRepository.Insert(new Payment
                    {
                        MemberId = memberId.Value,
                        PaymentDate = DateTime.Today,
                        Amount = result.Amount > 0 ? result.Amount : 0,
                        PaymentMode = PaymentGatewayNames.Paytm,
                        ReferenceNo = result.TransactionId,
                        Remarks = remarks
                    });
                }

                HttpContext.Session.Remove("PendingPaymentMemberId");
                HttpContext.Session.Remove("PendingPaymentFor");
                HttpContext.Session.Remove("PendingPaymentRemarks");

                TempData["Success"] = "Payment completed successfully.";
                return RedirectToAction("Index", "Payments");
            }

            TempData["Error"] = result.ResponseMessage;
            return RedirectToAction("Index", "Payments");
        }

        [HttpGet]
        public IActionResult Checkout(string orderId, string txnToken, string amount)
        {
            return RedirectToAction("Checkout", "OnlinePayment", new
            {
                gateway = PaymentGatewayNames.Paytm,
                orderId,
                txnToken,
                amount
            });
        }
    }
}

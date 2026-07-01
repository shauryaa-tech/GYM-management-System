using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using GymManagement.Services.WhatsApp;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    [AllowAnonymous]
    public class PayController : Controller
    {
        private readonly WhatsAppBotSessionRepository _sessionRepo;
        private readonly MembershipRepository _membershipRepo;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly LeadRepository _leadRepo;

        public PayController(
            WhatsAppBotSessionRepository sessionRepo,
            MembershipRepository membershipRepo,
            IPaymentGatewayService paymentGatewayService,
            LeadRepository leadRepo)
        {
            _sessionRepo = sessionRepo;
            _membershipRepo = membershipRepo;
            _paymentGatewayService = paymentGatewayService;
            _leadRepo = leadRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Lead(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return View("PaymentError", "Invalid payment link.");

            var session = _sessionRepo.GetByPaymentToken(token);
            if (session == null || session.SelectedPlanId == null)
                return View("PaymentError", "Payment session not found. Please complete WhatsApp bot steps first.");

            var plan = _membershipRepo.GetById(session.SelectedPlanId.Value);
            if (plan == null)
                return View("PaymentError", "Membership plan not found.");

            var amount = plan.Amount + plan.JoiningFee;
            if (amount <= 0)
                return View("PaymentError", "Invalid plan amount.");

            var orderId = "LEAD" + DateTime.UtcNow.Ticks;
            var request = new PaymentOrderRequest
            {
                OrderId = orderId,
                Amount = amount,
                Currency = "INR",
                CustomerId = session.LeadId.ToString(),
                CustomerPhone = session.PhoneNumber,
                PaymentFor = $"LeadBot:{session.LeadId}:{session.SelectedPlanId}:{session.SelectedTrainerId}:{session.SelectedClassId}"
            };

            var result = await _paymentGatewayService.InitiatePaymentAsync(request);
            if (!result.Success)
                return View("PaymentError", result.Message);

            HttpContext.Session.SetString("PublicPaymentToken", token);
            HttpContext.Session.SetString("PublicPaymentFor", request.PaymentFor);
            HttpContext.Session.SetInt32("PublicPaymentLeadId", session.LeadId);

            return RedirectToAction("Checkout", "OnlinePayment", new
            {
                gateway = result.Gateway,
                orderId = result.OrderId,
                txnToken = result.TransactionToken,
                amount = amount.ToString("0.00"),
                redirectUrl = result.RedirectUrl,
                razorPayOrderId = result.RazorPayOrderId,
                razorPayKeyId = result.RazorPayKeyId,
                cashfreeSessionId = result.CashfreeSessionId
            });
        }
    }
}

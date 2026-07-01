using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Repositories;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using GymManagement.Helpers;
using GymManagement.Services.PaymentProviders;
using GymManagement.Services.WhatsApp;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    [AllowAnonymous]
    public class OnlinePaymentController : Controller
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly PaymentRepository _paymentRepository;
        private readonly MembershipTransactionRepository _membershipTxnRepo;
        private readonly SalaryProcessingRepository _salaryRepo;
        private readonly WhatsAppBotSessionRepository _botSessionRepo;
        private readonly LeadRepository _leadRepo;
        private readonly IWhatsAppService _whatsAppService;

        public OnlinePaymentController(
            IPaymentGatewayService paymentGatewayService,
            PaymentRepository paymentRepository,
            MembershipTransactionRepository membershipTxnRepo,
            SalaryProcessingRepository salaryRepo,
            WhatsAppBotSessionRepository botSessionRepo,
            LeadRepository leadRepo,
            IWhatsAppService whatsAppService)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentRepository = paymentRepository;
            _membershipTxnRepo = membershipTxnRepo;
            _salaryRepo = salaryRepo;
            _botSessionRepo = botSessionRepo;
            _leadRepo = leadRepo;
            _whatsAppService = whatsAppService;
        }

        [HttpGet]
        public IActionResult Checkout(
            string gateway,
            string orderId,
            string? txnToken,
            string? amount,
            string? redirectUrl,
            string? razorPayOrderId,
            string? razorPayKeyId,
            string? cashfreeSessionId,
            string? merchantId,
            string? environment)
        {
            ViewBag.Gateway = gateway;
            ViewBag.OrderId = orderId;
            ViewBag.TxnToken = txnToken;
            ViewBag.Amount = amount;
            ViewBag.RedirectUrl = redirectUrl;
            ViewBag.RazorPayKeyId = razorPayKeyId;
            ViewBag.RazorPayOrderId = razorPayOrderId;
            ViewBag.CashfreeSessionId = cashfreeSessionId;
            ViewBag.MerchantId = merchantId;
            ViewBag.Environment = environment ?? "Sandbox";
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public Task<IActionResult> PaytmCallback() =>
            HandleCallback(PaymentGatewayNames.Paytm);

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public Task<IActionResult> PhonePeCallback() =>
            HandleCallback(PaymentGatewayNames.PhonePe);

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public Task<IActionResult> RazorpayCallback() =>
            HandleCallback(PaymentGatewayNames.Razorpay);

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public Task<IActionResult> CashfreeCallback() =>
            HandleCallback(PaymentGatewayNames.Cashfree);

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public Task<IActionResult> CashfreeReturn() =>
            HandleCallback(PaymentGatewayNames.Cashfree);

        private async Task<IActionResult> HandleCallback(string gatewayName)
        {
            var callbackData = Request.HasFormContentType
                ? Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase)
                : Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            string? rawBody = null;
            if (Request.ContentLength > 0 && Request.Body.CanRead)
            {
                Request.EnableBuffering();
                using var reader = new StreamReader(Request.Body, leaveOpen: true);
                rawBody = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
            }

            var result = await _paymentGatewayService.ProcessCallbackAsync(gatewayName, callbackData, rawBody);

            if (result.Success)
            {
                var publicPaymentFor = HttpContext.Session.GetString("PublicPaymentFor");
                var publicLeadId = HttpContext.Session.GetInt32("PublicPaymentLeadId");
                var publicToken = HttpContext.Session.GetString("PublicPaymentToken");

                if (!string.IsNullOrWhiteSpace(publicPaymentFor) &&
                    publicPaymentFor.StartsWith("LeadBot:", StringComparison.OrdinalIgnoreCase) &&
                    publicLeadId.HasValue)
                {
                    await CompletePublicLeadPaymentAsync(
                        publicLeadId.Value,
                        publicToken,
                        result,
                        gatewayName);

                    HttpContext.Session.Remove("PublicPaymentFor");
                    HttpContext.Session.Remove("PublicPaymentLeadId");
                    HttpContext.Session.Remove("PublicPaymentToken");

                    return RedirectToAction("Success", "PublicJoin", new { leadId = publicLeadId.Value, paid = true });
                }

                var memberId = HttpContext.Session.GetInt32("PendingPaymentMemberId");
                var paymentFor = HttpContext.Session.GetString("PendingPaymentFor") ?? "Online Payment";
                var remarks = HttpContext.Session.GetString("PendingPaymentRemarks") ?? paymentFor;

                var salaryId = HttpContext.Session.GetInt32("PendingSalaryId");
                if (salaryId.HasValue)
                {
                    var salary = _salaryRepo.GetById(salaryId.Value);
                    if (salary.SalaryId > 0)
                    {
                        var salaryRemarks = salary.Remarks ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(result.TransactionId))
                        {
                            salaryRemarks = string.IsNullOrWhiteSpace(salaryRemarks)
                                ? $"Txn: {result.TransactionId}"
                                : $"{salaryRemarks} | Txn: {result.TransactionId}";
                        }

                        _salaryRepo.MarkPaid(salary.SalaryId, DateTime.Today, gatewayName, salaryRemarks);
                    }

                    HttpContext.Session.Remove("PendingSalaryId");
                    HttpContext.Session.Remove("PendingSalaryPaymentFor");

                    TempData["Success"] = $"Salary paid successfully via {gatewayName}.";
                    return RedirectToAction("Index", "SalaryProcessing");
                }

                if (memberId.HasValue)
                {
                    _paymentRepository.Insert(new Payment
                    {
                        MemberId = memberId.Value,
                        PaymentDate = DateTime.Today,
                        Amount = result.Amount > 0 ? result.Amount : 0,
                        PaymentMode = gatewayName,
                        ReferenceNo = result.TransactionId,
                        Remarks = remarks
                    });
                }

                var membershipTxnId = HttpContext.Session.GetInt32("PendingMembershipTransactionId");
                if (membershipTxnId.HasValue)
                {
                    _membershipTxnRepo.UpdatePaymentStatus(
                        membershipTxnId.Value,
                        "Paid",
                        $"Paid online via {gatewayName} on {DateTime.Now:dd-MMM-yyyy}. Txn: {result.TransactionId}");
                }

                HttpContext.Session.Remove("PendingPaymentMemberId");
                HttpContext.Session.Remove("PendingMembershipTransactionId");
                HttpContext.Session.Remove("PendingPaymentFor");
                HttpContext.Session.Remove("PendingPaymentRemarks");

                TempData["Success"] = membershipTxnId.HasValue
                    ? "Online payment successful. Membership marked as Paid."
                    : "Payment completed successfully.";
                return RedirectToAction("Index", membershipTxnId.HasValue ? "MembershipManagement" : "Payments");
            }

            var failedSalaryId = HttpContext.Session.GetInt32("PendingSalaryId");
            TempData["Error"] = failedSalaryId.HasValue
                ? "Online payment failed. Salary status Pending hai — dubara Pay Online try karein ya Mark as Paid karein."
                : result.ResponseMessage;
            HttpContext.Session.Remove("PendingSalaryId");
            HttpContext.Session.Remove("PendingSalaryPaymentFor");
            return RedirectToAction("Index", failedSalaryId.HasValue ? "SalaryProcessing" : "Payments");
        }

        private async Task CompletePublicLeadPaymentAsync(
            int leadId,
            string? paymentToken,
            PaymentVerificationResult result,
            string gatewayName)
        {
            var lead = _leadRepo.GetById(leadId);
            if (lead.LeadId > 0)
            {
                lead.Status = "Converted";
                lead.IsConverted = true;
                lead.Remarks = (lead.Remarks ?? string.Empty) + $" | Paid via {gatewayName} on {DateTime.Now:dd-MMM-yyyy}";
                _leadRepo.Update(lead);
            }

            var session = !string.IsNullOrWhiteSpace(paymentToken)
                ? _botSessionRepo.GetByPaymentToken(paymentToken)
                : _botSessionRepo.GetByLeadId(leadId);

            if (session != null)
            {
                session.IsCompleted = true;
                session.CurrentStep = WhatsAppBotSteps.Completed;
                _botSessionRepo.Update(session);

                await _whatsAppService.SendTextAsync(
                    session.PhoneNumber,
                    $"Payment successful! Thank you for joining CloudMex Gym.\nOrder: {result.OrderId}\nAmount: ₹{result.Amount:N0}\n\nOur team will contact you shortly.");
            }
        }
    }
}

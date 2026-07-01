using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.Repositories;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PaymentRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly MembershipTransactionRepository _membershipTxnRepo;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public PaymentsController(
            PaymentRepository repo,
            MemberRepository memberRepo,
            MembershipTransactionRepository membershipTxnRepo,
            IPaymentGatewayService paymentGatewayService)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _membershipTxnRepo = membershipTxnRepo;
            _paymentGatewayService = paymentGatewayService;
        }

        [Permission("Payments", "View")]
        public IActionResult Index(DateTime? fromDate, DateTime? toDate, string? mode)
        {
            ViewBag.Members = _memberRepo.GetAll(null, "Active");
            
            var payments = _repo.GetAll(fromDate, toDate, mode);

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Mode = mode;

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Payments", "Add")]
        public IActionResult Save(Payment model, int? membershipTransactionId)
        {
            try
            {
                if (model.PaymentDate == DateTime.MinValue)
                    model.PaymentDate = DateTime.Today;

                _repo.Insert(model);

                if (membershipTransactionId.HasValue && membershipTransactionId > 0)
                {
                    _membershipTxnRepo.UpdatePaymentStatus(
                        membershipTransactionId.Value,
                        "Paid",
                        $"Paid via {model.PaymentMode} on {model.PaymentDate:dd-MMM-yyyy}");
                }

                TempData["Success"] = membershipTransactionId.HasValue
                    ? "Payment saved and membership marked as Paid."
                    : "Payment processed successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Payments", "Add")]
        public async Task<IActionResult> PayOnline(int memberId, decimal amount, string paymentFor, string? remarks, int? membershipTransactionId)
        {
            if (memberId <= 0 || amount <= 0)
            {
                TempData["Error"] = "Member and amount are required for online payment.";
                return RedirectToAction("Index");
            }

            var orderId = "GYM" + DateTime.UtcNow.Ticks;
            var request = new PaymentOrderRequest
            {
                OrderId = orderId,
                Amount = amount,
                CustomerId = memberId.ToString(),
                PaymentFor = string.IsNullOrWhiteSpace(paymentFor) ? "General Payment" : paymentFor,
                Currency = "INR"
            };

            var result = await _paymentGatewayService.InitiatePaymentAsync(request);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetInt32("PendingPaymentMemberId", memberId);
            HttpContext.Session.SetString("PendingPaymentFor", request.PaymentFor);
            HttpContext.Session.SetString("PendingPaymentRemarks", remarks ?? string.Empty);
            if (membershipTransactionId.HasValue && membershipTransactionId > 0)
                HttpContext.Session.SetInt32("PendingMembershipTransactionId", membershipTransactionId.Value);

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

        [Permission("Payments", "Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                TempData["Success"] = "Payment Record Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [Permission("Payments", "View")]
        public IActionResult Export() => this.ExportCsv("Payments");
    }
}

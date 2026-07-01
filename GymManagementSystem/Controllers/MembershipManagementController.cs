using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.Repositories;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class MembershipManagementController : Controller
    {
        private readonly MembershipTransactionRepository _repo;
        private readonly MemberRepository _memberRepo;
        private readonly MembershipRepository _planRepo;
        private readonly PaymentRepository _paymentRepo;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public MembershipManagementController(
            MembershipTransactionRepository repo,
            MemberRepository memberRepo,
            MembershipRepository planRepo,
            PaymentRepository paymentRepo,
            IPaymentGatewayService paymentGatewayService)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _planRepo = planRepo;
            _paymentRepo = paymentRepo;
            _paymentGatewayService = paymentGatewayService;
        }

        [Permission("MembershipManagement", "View")]
        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }

        [Permission("MembershipManagement", "Add")]
        public IActionResult Save()
        {
            ViewBag.Members = _memberRepo.GetAll(null, null);
            ViewBag.Plans = _planRepo.GetAll(null, true);
            return View(new MembershipTransaction { PaymentStatus = "Pending", StartDate = DateTime.Today });
        }

        [HttpPost]
        [Permission("MembershipManagement", "Add")]
        public IActionResult Save(MembershipTransaction model)
        {
            model.MembershipStatus = model.EndDate.Date >= DateTime.Today ? "Active" : "Expired";
            if (string.IsNullOrWhiteSpace(model.PaymentStatus))
                model.PaymentStatus = "Pending";

            _repo.Insert(model);
            TempData["Success"] = "Membership added. Record payment when member pays (cash) or use Pay Online.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("MembershipManagement", "Edit")]
        public IActionResult RecordCashPayment(
            int transactionId,
            string paymentMode,
            DateTime? paymentDate,
            string? referenceNo)
        {
            var txn = _repo.GetById(transactionId);
            if (txn == null)
            {
                TempData["Error"] = "Membership transaction not found.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(txn.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Payment already marked as done for this membership.";
                return RedirectToAction(nameof(Index));
            }

            var mode = string.IsNullOrWhiteSpace(paymentMode) ? "Cash" : paymentMode.Trim();
            var date = paymentDate ?? DateTime.Today;

            _paymentRepo.Insert(new Payment
            {
                MemberId = txn.MemberId,
                PaymentDate = date,
                Amount = txn.Amount,
                PaymentMode = mode,
                ReferenceNo = referenceNo,
                Remarks = $"Membership #{txn.TransactionId} - {txn.PlanName} (Cash/Manual)"
            });

            _repo.UpdatePaymentStatus(
                transactionId,
                "Paid",
                $"{txn.Remarks} | Paid via {mode} on {date:dd-MMM-yyyy}");

            TempData["Success"] = $"Payment recorded ({mode}). Membership marked as Paid.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("MembershipManagement", "Edit")]
        public async Task<IActionResult> PayOnline(int transactionId)
        {
            var txn = _repo.GetById(transactionId);
            if (txn == null)
            {
                TempData["Error"] = "Membership transaction not found.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(txn.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "This membership is already paid.";
                return RedirectToAction(nameof(Index));
            }

            if (txn.Amount <= 0)
            {
                TempData["Error"] = "Invalid membership amount.";
                return RedirectToAction(nameof(Index));
            }

            var orderId = "MT" + txn.TransactionId + "-" + DateTime.UtcNow.Ticks;
            var paymentFor = $"Membership #{txn.TransactionId}";

            var result = await _paymentGatewayService.InitiatePaymentAsync(new PaymentOrderRequest
            {
                OrderId = orderId,
                Amount = txn.Amount,
                CustomerId = txn.MemberId.ToString(),
                PaymentFor = paymentFor,
                Currency = "INR"
            });

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetInt32("PendingPaymentMemberId", txn.MemberId);
            HttpContext.Session.SetInt32("PendingMembershipTransactionId", txn.TransactionId);
            HttpContext.Session.SetString("PendingPaymentFor", paymentFor);
            HttpContext.Session.SetString("PendingPaymentRemarks", $"{txn.PlanName} - Online payment");

            return RedirectToAction("Checkout", "OnlinePayment", new
            {
                gateway = result.Gateway,
                orderId = result.OrderId,
                txnToken = result.TransactionToken,
                amount = txn.Amount.ToString("0.00"),
                redirectUrl = result.RedirectUrl,
                razorPayOrderId = result.RazorPayOrderId,
                razorPayKeyId = result.RazorPayKeyId,
                cashfreeSessionId = result.CashfreeSessionId,
                merchantId = result.MerchantId,
                environment = result.Environment
            });
        }

        [Permission("MembershipManagement", "Delete")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "Membership deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("MembershipManagement", "View")]
        public IActionResult Export() => this.ExportCsv("MembershipManagement");
    }
}

using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.Services;
using GymManagement.Services.Interfaces;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class SalaryProcessingController : Controller
    {
        private readonly SalaryProcessingRepository _repo;
        private readonly SalaryRuleMasterRepository _ruleRepo;
        private readonly SalaryCalculationService _calcService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly SalaryStatementService _statementService;
        private readonly BankStatementService _bankStatementService;
        private readonly IConfiguration _config;

        public SalaryProcessingController(
            SalaryProcessingRepository repo,
            SalaryRuleMasterRepository ruleRepo,
            SalaryCalculationService calcService,
            IPaymentGatewayService paymentGatewayService,
            SalaryStatementService statementService,
            BankStatementService bankStatementService,
            IConfiguration config)
        {
            _repo = repo;
            _ruleRepo = ruleRepo;
            _calcService = calcService;
            _paymentGatewayService = paymentGatewayService;
            _statementService = statementService;
            _bankStatementService = bankStatementService;
            _config = config;
        }

        [Permission("SalaryProcessing", "View")]
        public IActionResult Index(string? search, string? staffId, string? month, string? year, string? paymentStatus)
        {
            ViewBag.Staff = _repo.GetActiveStaff();
            ViewBag.SalaryRules = _ruleRepo.GetActive();

            var salaries = _repo.GetAll(search, staffId, month, year, paymentStatus);

            ViewBag.Search = search;
            ViewBag.StaffId = staffId;
            ViewBag.Month = month ?? DateTime.Today.Month.ToString();
            ViewBag.Year = year ?? DateTime.Today.Year.ToString();
            ViewBag.PaymentStatus = paymentStatus;

            return View(salaries);
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public async Task<IActionResult> Generate(int staffId, int? month, int? year, int? ruleId)
        {
            if (staffId <= 0)
            {
                TempData["Error"] = "Select a staff member.";
                return RedirectToAction(nameof(Index));
            }

            if (!_ruleRepo.GetActive().Any())
            {
                TempData["Error"] = "Pehle Masters → Salary Rule Master se rule add karein.";
                return RedirectToAction("Index", "SalaryRuleMaster");
            }

            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            try
            {
                ViewBag.SalaryRules = _ruleRepo.GetActive();
                ViewBag.DefaultGateway = await _paymentGatewayService.GetDefaultGatewayAsync();
                return View(_calcService.Calculate(staffId, m, y, ruleId));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public JsonResult PreviewSalary(int staffId, int month, int year, int? ruleId)
        {
            try
            {
                var s = _calcService.Calculate(staffId, month, year, ruleId);
                return Json(new
                {
                    success = true,
                    ruleName = s.RuleName,
                    basicSalary = s.BasicSalary,
                    deductions = s.Deductions,
                    netSalary = s.NetSalary,
                    presentDays = s.PresentDays,
                    absentDays = s.AbsentDays,
                    leaveDays = s.LeaveDays,
                    halfDays = s.HalfDays,
                    perDaySalary = s.PerDaySalary,
                    deductionDays = s.DeductionDays,
                    alreadyProcessed = s.AlreadyProcessed,
                    isPaid = s.IsPaid,
                    isPaymentPending = s.IsPaymentPending,
                    paymentStatus = s.IsPaid ? "Paid" : s.IsPaymentPending ? "Pending" : "",
                    ruleSummary = s.RuleSummary
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public IActionResult GenerateAndSave(int staffId, int month, int year, int? ruleId, string? paymentMode, DateTime? paidDate, string? remarks)
        {
            try
            {
                var summary = _calcService.Calculate(staffId, month, year, ruleId);
                if (summary.IsPaid)
                {
                    TempData["Error"] = "Salary already paid for this staff and month.";
                    return RedirectToAction(nameof(Generate), new { staffId, month, year, ruleId });
                }

                if (summary.IsPaymentPending && summary.ExistingSalaryId.HasValue)
                {
                    _repo.MarkPaid(
                        summary.ExistingSalaryId.Value,
                        paidDate ?? DateTime.Today,
                        paymentMode ?? "Cash",
                        remarks);
                    TempData["Success"] = $"Salary marked as paid for {summary.StaffName} — Net: ₹{summary.NetSalary:N0}";
                    return RedirectToAction(nameof(Index));
                }

                if (summary.HasSalaryRecord)
                {
                    TempData["Error"] = "Salary already processed for this staff and month.";
                    return RedirectToAction(nameof(Generate), new { staffId, month, year, ruleId });
                }

                _repo.InsertWithBreakdown(_calcService.ToSalaryRecord(summary, paymentMode, paidDate, remarks));
                TempData["Success"] = $"Salary generated for {summary.StaffName} — Net: ₹{summary.NetSalary:N0}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public async Task<IActionResult> GenerateAndPayOnline(int staffId, int month, int year, int? ruleId, string? remarks)
        {
            try
            {
                var summary = _calcService.Calculate(staffId, month, year, ruleId);
                if (summary.IsPaid)
                {
                    TempData["Error"] = "Salary already paid for this staff and month.";
                    return RedirectToAction(nameof(Generate), new { staffId, month, year, ruleId });
                }

                if (summary.IsPaymentPending && summary.ExistingSalaryId.HasValue)
                    return await PayOnlineInternal(summary.ExistingSalaryId.Value);

                if (summary.HasSalaryRecord)
                {
                    TempData["Error"] = "Salary already processed for this staff and month.";
                    return RedirectToAction(nameof(Generate), new { staffId, month, year, ruleId });
                }

                var record = _calcService.ToSalaryRecord(summary, "Pending", null, remarks);
                var salaryId = _repo.InsertWithBreakdownAndGetId(record);
                return await PayOnlineInternal(salaryId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Generate), new { staffId, month, year, ruleId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Edit")]
        public IActionResult MarkAsPaid(int salaryId, DateTime? paidDate, string? paymentMode, string? remarks)
        {
            try
            {
                var salary = _repo.GetById(salaryId);
                if (salary.SalaryId <= 0)
                {
                    TempData["Error"] = "Salary record not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (SalaryCalculationService.IsSalaryPaid(salary))
                {
                    TempData["Error"] = "This salary is already marked as paid.";
                    return RedirectToAction(nameof(Index));
                }

                var mode = string.IsNullOrWhiteSpace(paymentMode) ? "Cash" : paymentMode;
                var note = string.IsNullOrWhiteSpace(remarks) ? salary.Remarks : remarks;
                _repo.MarkPaid(salaryId, paidDate ?? DateTime.Today, mode, note);
                TempData["Success"] = $"Salary marked as paid ({mode}) — ₹{salary.NetSalary:N0}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public async Task<IActionResult> PayOnline(int salaryId) => await PayOnlineInternal(salaryId);

        private async Task<IActionResult> PayOnlineInternal(int salaryId)
        {
            var salary = _repo.GetById(salaryId);
            if (salary.SalaryId <= 0)
            {
                TempData["Error"] = "Salary record not found.";
                return RedirectToAction(nameof(Index));
            }

            if (salary.PaidDate.HasValue &&
                !SalaryCalculationService.IsSalaryPending(salary))
            {
                TempData["Error"] = "This salary is already marked as paid.";
                return RedirectToAction(nameof(Index));
            }

            if (salary.NetSalary <= 0)
            {
                TempData["Error"] = "Invalid salary amount for online payment.";
                return RedirectToAction(nameof(Index));
            }

            var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var period = $"{monthNames.ElementAtOrDefault(salary.Month) ?? salary.Month.ToString()} {salary.Year}";
            var orderId = "SAL" + salary.SalaryId + "-" + DateTime.UtcNow.Ticks;
            var paymentFor = $"Staff Salary - {salary.StaffName} ({period})";

            var result = await _paymentGatewayService.InitiatePaymentAsync(new PaymentOrderRequest
            {
                OrderId = orderId,
                Amount = salary.NetSalary,
                CustomerId = salary.StaffId.ToString(),
                PaymentFor = paymentFor,
                Currency = "INR"
            });

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.Remove("PendingPaymentMemberId");
            HttpContext.Session.Remove("PendingMembershipTransactionId");
            HttpContext.Session.Remove("PendingPaymentFor");
            HttpContext.Session.Remove("PendingPaymentRemarks");
            HttpContext.Session.SetInt32("PendingSalaryId", salary.SalaryId);
            HttpContext.Session.SetString("PendingSalaryPaymentFor", paymentFor);

            return RedirectToAction("Checkout", "OnlinePayment", new
            {
                gateway = result.Gateway,
                orderId = result.OrderId,
                txnToken = result.TransactionToken,
                amount = salary.NetSalary.ToString("0.00"),
                redirectUrl = result.RedirectUrl,
                razorPayOrderId = result.RazorPayOrderId,
                razorPayKeyId = result.RazorPayKeyId,
                cashfreeSessionId = result.CashfreeSessionId,
                merchantId = result.MerchantId,
                environment = result.Environment
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public IActionResult Save(SalaryProcessing model)
        {
            if (model.NetSalary == 0)
                model.NetSalary = model.BasicSalary - model.Deductions;

            _repo.Insert(model);
            TempData["Success"] = "Salary processed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("SalaryProcessing", "Edit")]
        public IActionResult Edit(int id)
        {
            ViewBag.Staff = _repo.GetActiveStaff();
            return PartialView("_EditSalaryProcessing", _repo.GetById(id));
        }

        [HttpPost]
        [Permission("SalaryProcessing", "Edit")]
        public IActionResult Update(SalaryProcessing model)
        {
            if (model.NetSalary == 0)
                model.NetSalary = model.BasicSalary - model.Deductions;

            _repo.Update(model);
            TempData["Success"] = "Salary updated.";
            return RedirectToAction(nameof(Index));
        }

        [Permission("SalaryProcessing", "Delete")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "Salary record deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult GetStaffSalary(int staffId)
        {
            var staff = _repo.GetActiveStaff().FirstOrDefault(s => s.StaffId == staffId);
            return Json(new { basicSalary = staff?.Salary ?? 0 });
        }

        [Permission("SalaryProcessing", "View")]
        public IActionResult Export(string? search, string? staffId, string? month, string? year)
        {
            var items = _repo.GetAll(search, staffId, month, year);
            var monthNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;

            var exportMonth = int.TryParse(month, out var parsedMonth)
                ? parsedMonth
                : items.FirstOrDefault()?.Month ?? DateTime.Today.Month;
            var exportYear = int.TryParse(year, out var parsedYear)
                ? parsedYear
                : items.FirstOrDefault()?.Year ?? DateTime.Today.Year;
            var periodLabel = exportMonth >= 1 && exportMonth <= 12
                ? $"{monthNames[exportMonth - 1]} {exportYear}"
                : $"{exportMonth}/{exportYear}";

            var columns = new ExcelColumnSpec[]
            {
                new("Staff Code", ExcelCellKind.Text),
                new("Staff Name", ExcelCellKind.Text),
                new("Designation", ExcelCellKind.Text),
                new("Basic Salary", ExcelCellKind.Currency),
                new("Deductions", ExcelCellKind.Currency),
                new("Net Salary", ExcelCellKind.Currency),
                new("Present Days", ExcelCellKind.Integer),
                new("Absent Days", ExcelCellKind.Integer),
                new("Leave Days", ExcelCellKind.Integer),
                new("Half Days", ExcelCellKind.Integer),
                new("Paid Date", ExcelCellKind.Date),
                new("Payment Mode", ExcelCellKind.Text),
                new("Remarks", ExcelCellKind.Text)
            };

            var rows = items.Select(s => new object?[]
            {
                s.DisplayStaffCode,
                s.StaffName,
                s.Designation,
                s.BasicSalary,
                s.Deductions,
                s.NetSalary,
                s.PresentDays ?? 0,
                s.AbsentDays ?? 0,
                s.LeaveDays ?? 0,
                s.HalfDays ?? 0,
                s.PaidDate,
                s.PaymentMode,
                s.Remarks
            });

            var label = !string.IsNullOrWhiteSpace(month) && !string.IsNullOrWhiteSpace(year)
                ? $"salary-processing-{month}-{year}"
                : "salary-processing";

            var reportHeader = new ExcelReportHeader(
                (_config["Gym:Name"] ?? "").Trim(),
                (_config["Gym:Address"] ?? "").Trim(),
                null,
                periodLabel);

            return ExcelHelper.ToFileResult(label, "Salary Processing", reportHeader, columns, rows);
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public IActionResult Statement(int? month, int? year, int? ruleId)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            try
            {
                ViewBag.SalaryRules = _ruleRepo.GetActive();
                return View(_statementService.Build(m, y, ruleId));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public IActionResult ExportStatement(int month, int year, int? ruleId)
        {
            try
            {
                var report = _statementService.Build(month, year, ruleId);
                return SalaryStatementExcelExporter.Export(report);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public IActionResult BankStatement(
            int? month,
            int? year,
            DateTime? paymentDate,
            string? department,
            string? search,
            bool onlyWithBank = false,
            int? ruleId = null)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            try
            {
                return View(_bankStatementService.Build(
                    m, y, paymentDate, department, search, onlyWithBank, ruleId));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public IActionResult ExportBankStatement(
            int month,
            int year,
            DateTime? paymentDate,
            string? department,
            string? search,
            bool onlyWithBank = false,
            int? ruleId = null)
        {
            try
            {
                var report = _bankStatementService.Build(
                    month, year, paymentDate, department, search, onlyWithBank, ruleId);

                var columns = new ExcelColumnSpec[]
                {
                    new("Emp Code", ExcelCellKind.Text),
                    new("Employee Name", ExcelCellKind.Text),
                    new("Company Name", ExcelCellKind.Text),
                    new("Bank Name", ExcelCellKind.Text),
                    new("Bank A/c No", ExcelCellKind.Text),
                    new("IFSC Code", ExcelCellKind.Text),
                    new("Amount", ExcelCellKind.Currency)
                };

                var rows = report.Rows.Select(r => new object?[]
                {
                    r.EmpCode, r.EmployeeName, r.CompanyName, r.BankName, r.BankAccountNo, r.IfscCode, r.Amount
                });

                var reportHeader = new ExcelReportHeader(
                    report.CompanyName,
                    null,
                    null,
                    report.MonthLabel);

                return ExcelHelper.ToFileResult(
                    $"bank-statement-{report.MonthLabel}",
                    "Bank Statement",
                    reportHeader,
                    columns,
                    rows);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public IActionResult ExportStatementCsv() => this.ExportCsv("SalaryStatement");

        [HttpGet]
        [Permission("SalaryProcessing", "View")]
        public IActionResult ExportBankStatementCsv() => this.ExportCsv("BankStatement");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public IActionResult Import(IFormFile? file) => this.ImportCsv("SalaryProcessing", file);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public IActionResult ImportStatement(IFormFile? file, int? month, int? year) =>
            this.ImportCsv("SalaryStatement", file, "Statement", new { month, year });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("SalaryProcessing", "Add")]
        public IActionResult ImportBankStatement(IFormFile? file, int? month, int? year) =>
            this.ImportCsv("BankStatement", file, "BankStatement", new { month, year });
    }
}

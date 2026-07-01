using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.ViewModels;

namespace GymManagement.Services
{
    public class SalaryCalculationService
    {
        private readonly StaffAttendanceRepository _attendanceRepo;
        private readonly SalaryProcessingRepository _salaryRepo;
        private readonly SalaryRuleMasterRepository _ruleRepo;

        public SalaryCalculationService(
            StaffAttendanceRepository attendanceRepo,
            SalaryProcessingRepository salaryRepo,
            SalaryRuleMasterRepository ruleRepo)
        {
            _attendanceRepo = attendanceRepo;
            _salaryRepo = salaryRepo;
            _ruleRepo = ruleRepo;
        }

        public StaffAttendanceSummary Calculate(int staffId, int month, int year, int? ruleId = null)
        {
            var staff = _attendanceRepo.GetActiveStaff().FirstOrDefault(s => s.StaffId == staffId)
                ?? throw new InvalidOperationException("Staff not found.");

            var rules = _ruleRepo.ResolveRule(ruleId)
                ?? throw new InvalidOperationException("No salary rule found. Please add a rule in Masters → Salary Rule Master.");

            var counts = _attendanceRepo.GetMonthlyCounts(staffId, month, year);
            var daily = _attendanceRepo.GetMonthlyAttendance(staffId, month, year);

            int workingDays = rules.WorkingDaysPerMonth > 0 ? rules.WorkingDaysPerMonth : 26;
            decimal perDay = staff.Salary / workingDays;

            decimal deductionDays = counts.AbsentDays;
            deductionDays += counts.HalfDays * rules.HalfDayDeductionFactor;
            if (!rules.LeaveIsPaid)
                deductionDays += counts.LeaveDays;

            var shiftSandwich = SalaryRuleEngine.ApplyShiftAndSandwich(rules, daily, month, year);
            deductionDays += shiftSandwich.TotalExtraDeductionDays;

            if (!rules.AbsentDeductionPerDay)
                deductionDays = 0;

            decimal deductions = Math.Round(perDay * deductionDays, 2);
            decimal net = Math.Max(0, staff.Salary - deductions);

            var existing = _salaryRepo.GetByStaffPeriod(staffId, month, year);
            var isPaid = existing != null && IsSalaryPaid(existing);
            var isPending = existing != null && IsSalaryPending(existing);

            return new StaffAttendanceSummary
            {
                StaffId = staff.StaffId,
                StaffName = staff.StaffName,
                Designation = staff.Designation,
                BasicSalary = staff.Salary,
                Month = month,
                Year = year,
                RuleId = rules.RuleId,
                RuleName = rules.RuleName,
                PresentDays = counts.PresentDays,
                AbsentDays = counts.AbsentDays,
                LeaveDays = counts.LeaveDays,
                HalfDays = counts.HalfDays,
                TotalMarkedDays = counts.TotalMarked,
                WorkingDaysPerMonth = workingDays,
                PerDaySalary = Math.Round(perDay, 2),
                DeductionDays = deductionDays,
                Deductions = deductions,
                NetSalary = net,
                LatePenaltyDays = shiftSandwich.LatePenaltyDays,
                EarlyLeavePenaltyDays = shiftSandwich.EarlyLeavePenaltyDays,
                SandwichAbsentDays = shiftSandwich.SandwichAbsentDays,
                AlreadyProcessed = isPaid,
                HasSalaryRecord = existing != null,
                IsPaid = isPaid,
                IsPaymentPending = isPending,
                ExistingSalaryId = existing?.SalaryId,
                ExistingPaymentMode = existing?.PaymentMode,
                ExistingPaidDate = existing?.PaidDate,
                DailyRows = daily,
                RuleSummary = BuildRuleSummary(rules, workingDays, perDay, shiftSandwich)
            };
        }

        public SalaryProcessing ToSalaryRecord(StaffAttendanceSummary summary, string? paymentMode, DateTime? paidDate, string? remarks)
        {
            return new SalaryProcessing
            {
                StaffId = summary.StaffId,
                Month = summary.Month,
                Year = summary.Year,
                BasicSalary = summary.BasicSalary,
                Deductions = summary.Deductions,
                NetSalary = summary.NetSalary,
                PresentDays = summary.PresentDays,
                AbsentDays = summary.AbsentDays,
                LeaveDays = summary.LeaveDays,
                HalfDays = summary.HalfDays,
                SalaryRuleId = summary.RuleId,
                PaidDate = IsPendingOnlinePayment(paymentMode) ? null : (paidDate ?? DateTime.Today),
                PaymentMode = paymentMode ?? "Cash",
                Remarks = remarks ?? BuildAttendanceRemarks(summary)
            };
        }

        public static bool IsPendingOnlinePayment(string? paymentMode) =>
            string.Equals(paymentMode, "Pending", StringComparison.OrdinalIgnoreCase);

        public static bool IsSalaryPaid(SalaryProcessing? salary) =>
            salary != null && salary.SalaryId > 0 && salary.IsPaid;

        public static bool IsSalaryPending(SalaryProcessing? salary) =>
            salary != null && salary.SalaryId > 0 && salary.IsPaymentPending;

        private static string BuildAttendanceRemarks(StaffAttendanceSummary summary)
        {
            var parts = new List<string>
            {
                $"Rule: {summary.RuleName}",
                $"Present:{summary.PresentDays}",
                $"Absent:{summary.AbsentDays}",
                $"Leave:{summary.LeaveDays}",
                $"Half:{summary.HalfDays}"
            };

            if (summary.LatePenaltyDays > 0)
                parts.Add($"Late penalty:{summary.LatePenaltyDays:0.##}d");
            if (summary.EarlyLeavePenaltyDays > 0)
                parts.Add($"Early leave:{summary.EarlyLeavePenaltyDays:0.##}d");
            if (summary.SandwichAbsentDays > 0)
                parts.Add($"Sandwich off:{summary.SandwichAbsentDays}d");

            parts.Add($"Deduction days:{summary.DeductionDays:0.##}");

            return string.Join(" | ", parts);
        }

        private static string BuildRuleSummary(
            SalaryRuleMaster rules,
            int workingDays,
            decimal perDay,
            ShiftSandwichResult shift) =>
            $"[{rules.RuleName}] Shift: {rules.ShiftDisplay} | Working days: {workingDays} | Per day: ₹{perDay:N2} | " +
            $"Half-day factor: {rules.HalfDayDeductionFactor} | Leave paid: {(rules.LeaveIsPaid ? "Yes" : "No")} | " +
            $"Sandwich: {(rules.EnableSandwichRule ? "ON" : "OFF")}" +
            (shift.SandwichAbsentDays > 0 ? $" (+{shift.SandwichAbsentDays} off days)" : "") +
            (shift.LatePenaltyDays > 0 ? $" | Late penalty: {shift.LatePenaltyDays:0.##}d" : "") +
            (shift.EarlyLeavePenaltyDays > 0 ? $" | Early leave: {shift.EarlyLeavePenaltyDays:0.##}d" : "");
    }
}

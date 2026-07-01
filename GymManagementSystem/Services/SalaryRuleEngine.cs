using GymManagement.Models;
using GymManagement.ViewModels;

namespace GymManagement.Services
{
    public static class SalaryRuleEngine
    {
        public static ShiftSandwichResult ApplyShiftAndSandwich(
            SalaryRuleMaster rules,
            List<StaffAttendanceDayRow> daily,
            int month,
            int year)
        {
            var result = new ShiftSandwichResult();
            if (!rules.AbsentDeductionPerDay)
                return result;

            var statusByDate = daily.ToDictionary(d => d.Date.Date, d => d);
            var offDays = ParseWeeklyOffDays(rules.WeeklyOffDays);

            if (rules.ShiftStartTime.HasValue)
            {
                var lateCutoff = rules.ShiftStartTime.Value.Add(TimeSpan.FromMinutes(rules.LateGraceMinutes));
                foreach (var row in daily.Where(d => d.Status == "Present" && d.CheckInTime.HasValue))
                {
                    if (rules.LateCountsAsHalfDay && row.CheckInTime!.Value > lateCutoff)
                        result.LatePenaltyDays += rules.HalfDayDeductionFactor;
                }
            }

            if (rules.ShiftEndTime.HasValue)
            {
                var earlyCutoff = rules.ShiftEndTime.Value.Subtract(TimeSpan.FromMinutes(rules.EarlyLeaveGraceMinutes));
                if (earlyCutoff < TimeSpan.Zero)
                    earlyCutoff = TimeSpan.Zero;

                foreach (var row in daily.Where(d => d.Status == "Present" && d.CheckOutTime.HasValue))
                {
                    if (rules.EarlyLeaveCountsAsHalfDay && row.CheckOutTime!.Value < earlyCutoff)
                        result.EarlyLeavePenaltyDays += rules.HalfDayDeductionFactor;
                }
            }

            if (rules.EnableSandwichRule && offDays.Count > 0)
            {
                var daysInMonth = DateTime.DaysInMonth(year, month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    if (!offDays.Contains(date.DayOfWeek))
                        continue;

                    var prevWork = FindAdjacentWorkingDay(date, -1, month, year, offDays);
                    var nextWork = FindAdjacentWorkingDay(date, 1, month, year, offDays);
                    if (prevWork == null || nextWork == null)
                        continue;

                    if (IsAbsentLike(GetStatus(prevWork.Value, statusByDate), rules.LeaveIsPaid) &&
                        IsAbsentLike(GetStatus(nextWork.Value, statusByDate), rules.LeaveIsPaid))
                    {
                        result.SandwichAbsentDays++;
                    }
                }
            }

            result.TotalExtraDeductionDays =
                result.LatePenaltyDays +
                result.EarlyLeavePenaltyDays +
                result.SandwichAbsentDays;

            return result;
        }

        private static string GetStatus(DateTime date, Dictionary<DateTime, StaffAttendanceDayRow> map) =>
            map.TryGetValue(date.Date, out var row) ? row.Status : "NotMarked";

        private static bool IsAbsentLike(string status, bool leaveIsPaid) =>
            status == "Absent" || (status == "Leave" && !leaveIsPaid);

        private static DateTime? FindAdjacentWorkingDay(
            DateTime from,
            int direction,
            int month,
            int year,
            HashSet<DayOfWeek> offDays)
        {
            var cursor = from.AddDays(direction);
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            while (cursor >= monthStart && cursor <= monthEnd)
            {
                if (!offDays.Contains(cursor.DayOfWeek))
                    return cursor;
                cursor = cursor.AddDays(direction);
            }

            return null;
        }

        private static HashSet<DayOfWeek> ParseWeeklyOffDays(string? value)
        {
            var set = new HashSet<DayOfWeek>();
            if (string.IsNullOrWhiteSpace(value))
                return set;

            foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Enum.TryParse<DayOfWeek>(part, true, out var dow))
                    set.Add(dow);
            }

            return set;
        }

        public static int CountWeeklyOffDaysInMonth(string? weeklyOffDays, int month, int year)
        {
            var offDays = ParseWeeklyOffDays(weeklyOffDays);
            if (offDays.Count == 0) return 0;

            int count = 0;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int d = 1; d <= daysInMonth; d++)
            {
                if (offDays.Contains(new DateTime(year, month, d).DayOfWeek))
                    count++;
            }
            return count;
        }

        public static string FormatOtHours(List<StaffAttendanceDayRow> daily, SalaryRuleMaster? rules)
        {
            if (rules?.ShiftEndTime == null) return "00:00";

            double totalMinutes = 0;
            foreach (var row in daily.Where(d => d.Status == "Present" && d.CheckOutTime.HasValue))
            {
                var ot = row.CheckOutTime!.Value - rules.ShiftEndTime.Value;
                if (ot.TotalMinutes > 0)
                    totalMinutes += ot.TotalMinutes;
            }

            var ts = TimeSpan.FromMinutes(totalMinutes);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}";
        }

        public static decimal CalculatePayableDays(
            int presentDays,
            int leaveDays,
            int halfDays,
            SalaryRuleMaster rules)
        {
            decimal total = presentDays;
            if (rules.LeaveIsPaid)
                total += leaveDays;
            total += halfDays * rules.HalfDayDeductionFactor;
            return Math.Round(total, 2);
        }
    }

    public class ShiftSandwichResult
    {
        public decimal LatePenaltyDays { get; set; }
        public decimal EarlyLeavePenaltyDays { get; set; }
        public int SandwichAbsentDays { get; set; }
        public decimal TotalExtraDeductionDays { get; set; }
    }
}

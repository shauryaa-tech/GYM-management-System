namespace GymManagement.Data.Queries
{
    public static class SalaryRuleMasterQueries
    {
        public const string GetAll = @"
            SELECT * FROM SalaryRuleMasters WHERE 1=1";

        public const string GetActive = @"
            SELECT * FROM SalaryRuleMasters WHERE IsActive = 1 ORDER BY IsDefault DESC, RuleName";

        public const string GetById = @"
            SELECT * FROM SalaryRuleMasters WHERE RuleId = @RuleId";

        public const string GetDefault = @"
            SELECT TOP 1 * FROM SalaryRuleMasters
            WHERE IsActive = 1
            ORDER BY IsDefault DESC, RuleId";

        public const string Insert = @"
            INSERT INTO SalaryRuleMasters
            (RuleName, WorkingDaysPerMonth, AbsentDeductionPerDay, HalfDayDeductionFactor,
             LeaveIsPaid, LateGraceMinutes, ShiftStartTime, ShiftEndTime, EarlyLeaveGraceMinutes,
             EnableSandwichRule, WeeklyOffDays, LateCountsAsHalfDay, EarlyLeaveCountsAsHalfDay,
             Description, IsActive, IsDefault, CreatedDate)
            VALUES
            (@RuleName, @WorkingDaysPerMonth, @AbsentDeductionPerDay, @HalfDayDeductionFactor,
             @LeaveIsPaid, @LateGraceMinutes, @ShiftStartTime, @ShiftEndTime, @EarlyLeaveGraceMinutes,
             @EnableSandwichRule, @WeeklyOffDays, @LateCountsAsHalfDay, @EarlyLeaveCountsAsHalfDay,
             @Description, @IsActive, @IsDefault, GETDATE())";

        public const string Update = @"
            UPDATE SalaryRuleMasters SET
                RuleName = @RuleName,
                WorkingDaysPerMonth = @WorkingDaysPerMonth,
                AbsentDeductionPerDay = @AbsentDeductionPerDay,
                HalfDayDeductionFactor = @HalfDayDeductionFactor,
                LeaveIsPaid = @LeaveIsPaid,
                LateGraceMinutes = @LateGraceMinutes,
                ShiftStartTime = @ShiftStartTime,
                ShiftEndTime = @ShiftEndTime,
                EarlyLeaveGraceMinutes = @EarlyLeaveGraceMinutes,
                EnableSandwichRule = @EnableSandwichRule,
                WeeklyOffDays = @WeeklyOffDays,
                LateCountsAsHalfDay = @LateCountsAsHalfDay,
                EarlyLeaveCountsAsHalfDay = @EarlyLeaveCountsAsHalfDay,
                Description = @Description,
                IsActive = @IsActive,
                IsDefault = @IsDefault
            WHERE RuleId = @RuleId";

        public const string Delete = @"DELETE FROM SalaryRuleMasters WHERE RuleId = @RuleId";

        public const string ClearDefault = @"
            UPDATE SalaryRuleMasters SET IsDefault = 0 WHERE IsDefault = 1";

        public const string SetDefault = @"
            UPDATE SalaryRuleMasters SET IsDefault = 1 WHERE RuleId = @RuleId";
    }
}

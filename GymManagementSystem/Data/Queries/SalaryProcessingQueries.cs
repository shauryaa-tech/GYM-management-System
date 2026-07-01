namespace GymManagement.Data.Queries
{
    public static class SalaryProcessingQueries
    {
        public const string GetAll = @"
        SELECT
            SP.*,
            S.StaffName,
            S.StaffCode,
            S.Designation
        FROM SalaryProcessings SP
        LEFT JOIN StaffMasters S ON SP.StaffId = S.StaffId
        WHERE 1=1";

        public const string GetById = @"
        SELECT
            SP.*,
            S.StaffName,
            S.StaffCode,
            S.Designation
        FROM SalaryProcessings SP
        LEFT JOIN StaffMasters S ON SP.StaffId = S.StaffId
        WHERE SP.SalaryId=@SalaryId";

        public const string Insert = @"
        INSERT INTO SalaryProcessings
        (
            StaffId,
            Month,
            Year,
            BasicSalary,
            Deductions,
            NetSalary,
            PaidDate,
            PaymentMode,
            Remarks
        )
        VALUES
        (
            @StaffId,
            @Month,
            @Year,
            @BasicSalary,
            @Deductions,
            @NetSalary,
            @PaidDate,
            @PaymentMode,
            @Remarks
        )";

        public const string Update = @"
        UPDATE SalaryProcessings
        SET
            StaffId=@StaffId,
            Month=@Month,
            Year=@Year,
            BasicSalary=@BasicSalary,
            Deductions=@Deductions,
            NetSalary=@NetSalary,
            PaidDate=@PaidDate,
            PaymentMode=@PaymentMode,
            Remarks=@Remarks,
            PresentDays=@PresentDays,
            AbsentDays=@AbsentDays,
            LeaveDays=@LeaveDays,
            HalfDays=@HalfDays
        WHERE SalaryId=@SalaryId";

        public const string Delete =
            "DELETE FROM SalaryProcessings WHERE SalaryId=@SalaryId";

        public const string GetActiveStaff = @"
        SELECT StaffId, StaffName, Designation, Salary
        FROM StaffMasters
        WHERE IsActive=1
        ORDER BY StaffName";

        public const string ExistsForPeriod = @"
            SELECT COUNT(*) FROM SalaryProcessings
            WHERE StaffId = @StaffId AND Month = @Month AND Year = @Year";

        public const string GetByStaffPeriod = @"
        SELECT TOP 1
            SP.*,
            S.StaffName,
            S.StaffCode,
            S.Designation
        FROM SalaryProcessings SP
        LEFT JOIN StaffMasters S ON SP.StaffId = S.StaffId
        WHERE SP.StaffId = @StaffId AND SP.Month = @Month AND SP.Year = @Year
        ORDER BY SP.SalaryId DESC";

        public const string InsertWithBreakdown = @"
        INSERT INTO SalaryProcessings
        (StaffId, Month, Year, BasicSalary, Deductions, NetSalary, PaidDate, PaymentMode, Remarks, PresentDays, AbsentDays, LeaveDays, HalfDays, SalaryRuleId)
        VALUES
        (@StaffId, @Month, @Year, @BasicSalary, @Deductions, @NetSalary, @PaidDate, @PaymentMode, @Remarks, @PresentDays, @AbsentDays, @LeaveDays, @HalfDays, @SalaryRuleId);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public const string MarkPaid = @"
        UPDATE SalaryProcessings
        SET PaidDate=@PaidDate, PaymentMode=@PaymentMode, Remarks=@Remarks
        WHERE SalaryId=@SalaryId";
    }
}

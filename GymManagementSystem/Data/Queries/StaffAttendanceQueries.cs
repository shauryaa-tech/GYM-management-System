namespace GymManagement.Data.Queries
{
    public static class StaffAttendanceQueries
    {
        public const string GetAll = @"
        SELECT
            SA.*,
            S.StaffName,
            S.Designation
        FROM StaffAttendances SA
        LEFT JOIN StaffMasters S ON SA.StaffId = S.StaffId
        WHERE 1=1";

        public const string GetById = @"
        SELECT
            SA.*,
            S.StaffName,
            S.Designation
        FROM StaffAttendances SA
        LEFT JOIN StaffMasters S ON SA.StaffId = S.StaffId
        WHERE SA.AttendanceId=@AttendanceId";

        public const string Insert = @"
        INSERT INTO StaffAttendances
        (
            StaffId,
            AttendanceDate,
            CheckInTime,
            CheckOutTime,
            Status,
            Remarks
        )
        VALUES
        (
            @StaffId,
            @AttendanceDate,
            @CheckInTime,
            @CheckOutTime,
            @Status,
            @Remarks
        )";

        public const string Update = @"
        UPDATE StaffAttendances
        SET
            StaffId=@StaffId,
            AttendanceDate=@AttendanceDate,
            CheckInTime=@CheckInTime,
            CheckOutTime=@CheckOutTime,
            Status=@Status,
            Remarks=@Remarks
        WHERE AttendanceId=@AttendanceId";

        public const string Delete =
            "DELETE FROM StaffAttendances WHERE AttendanceId=@AttendanceId";

        public const string GetActiveStaff = @"
        SELECT StaffId, StaffName, Designation, Salary
        FROM StaffMasters
        WHERE IsActive=1
        ORDER BY StaffName";

        public const string GetTodayBoard = @"
            SELECT
                S.StaffId,
                S.StaffName,
                S.Designation,
                SA.AttendanceId,
                SA.Status AS DayStatus,
                SA.CheckInTime,
                SA.CheckOutTime,
                SA.Remarks
            FROM StaffMasters S
            LEFT JOIN StaffAttendances SA
                ON SA.StaffId = S.StaffId
               AND CAST(SA.AttendanceDate AS DATE) = CAST(@AttendanceDate AS DATE)
            WHERE S.IsActive = 1
            ORDER BY S.StaffName";

        public const string GetBoardForDate = GetTodayBoard;

        public const string GetByStaffAndDate = @"
            SELECT TOP 1 AttendanceId FROM StaffAttendances
            WHERE StaffId = @StaffId AND CAST(AttendanceDate AS DATE) = CAST(@AttendanceDate AS DATE)";

        public const string UpdateCheckOut = @"
            UPDATE StaffAttendances
            SET CheckOutTime = @CheckOutTime, Status = 'Present'
            WHERE AttendanceId = @AttendanceId";

        public const string QuickCheckIn = @"
            INSERT INTO StaffAttendances (StaffId, AttendanceDate, CheckInTime, Status)
            VALUES (@StaffId, CAST(GETDATE() AS DATE), @CheckInTime, 'Present')";

        public const string GetMonthlyAttendance = @"
            SELECT AttendanceId, AttendanceDate, Status, CheckInTime, CheckOutTime, Remarks
            FROM StaffAttendances
            WHERE StaffId = @StaffId
              AND MONTH(AttendanceDate) = @Month
              AND YEAR(AttendanceDate) = @Year
            ORDER BY AttendanceDate DESC";

        public const string GetByStaffDateRange = @"
            SELECT A.AttendanceId, A.AttendanceDate, A.Status, A.CheckInTime, A.CheckOutTime, A.Remarks,
                   S.StaffName, S.Designation
            FROM StaffAttendances A
            INNER JOIN StaffMasters S ON A.StaffId = S.StaffId
            WHERE A.StaffId = @StaffId
              AND A.AttendanceDate >= @FromDate
              AND A.AttendanceDate <= @ToDate
            ORDER BY A.AttendanceDate DESC";

        public const string GetMonthlyCounts = @"
            SELECT
                SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) AS PresentDays,
                SUM(CASE WHEN Status = 'Absent' THEN 1 ELSE 0 END) AS AbsentDays,
                SUM(CASE WHEN Status = 'Leave' THEN 1 ELSE 0 END) AS LeaveDays,
                SUM(CASE WHEN Status = 'HalfDay' THEN 1 ELSE 0 END) AS HalfDays,
                COUNT(*) AS TotalMarked
            FROM StaffAttendances
            WHERE StaffId = @StaffId
              AND MONTH(AttendanceDate) = @Month
              AND YEAR(AttendanceDate) = @Year";

        public const string GetMonthlySummaryAllStaff = @"
            SELECT
                S.StaffId,
                S.StaffName,
                S.Designation,
                S.Salary,
                ISNULL(A.PresentDays, 0) AS PresentDays,
                ISNULL(A.AbsentDays, 0) AS AbsentDays,
                ISNULL(A.LeaveDays, 0) AS LeaveDays,
                ISNULL(A.HalfDays, 0) AS HalfDays,
                ISNULL(A.TotalMarked, 0) AS TotalMarked
            FROM StaffMasters S
            LEFT JOIN (
                SELECT
                    StaffId,
                    SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) AS PresentDays,
                    SUM(CASE WHEN Status = 'Absent' THEN 1 ELSE 0 END) AS AbsentDays,
                    SUM(CASE WHEN Status = 'Leave' THEN 1 ELSE 0 END) AS LeaveDays,
                    SUM(CASE WHEN Status = 'HalfDay' THEN 1 ELSE 0 END) AS HalfDays,
                    COUNT(*) AS TotalMarked
                FROM StaffAttendances
                WHERE MONTH(AttendanceDate) = @Month AND YEAR(AttendanceDate) = @Year
                GROUP BY StaffId
            ) A ON S.StaffId = A.StaffId
            WHERE S.IsActive = 1
            ORDER BY S.StaffName";
    }
}

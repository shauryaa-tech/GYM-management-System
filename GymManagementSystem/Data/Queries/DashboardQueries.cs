namespace GymManagement.Data.Queries
{
    public static class DashboardQueries
    {
        public const string TotalMembers =
            "SELECT COUNT(*) FROM MemberMasters";

        public const string ActiveMembers =
            "SELECT COUNT(*) FROM MemberMasters WHERE Status='Active'";

        public const string TodayAttendance =
            @"SELECT COUNT(*)
              FROM Attendances
              WHERE CAST(AttendanceDate AS DATE) = CAST(@SelectedDate AS DATE)";

        public const string TotalRevenue =
            @"SELECT ISNULL(SUM(Amount),0)
              FROM MembershipTransactions";

        public const string TodayRevenue = @"
            SELECT ISNULL(SUM(C.Amount), 0)
            FROM (
                SELECT P.Amount
                FROM Payments P
                WHERE CAST(P.PaymentDate AS DATE) = CAST(@SelectedDate AS DATE)

                UNION ALL

                SELECT MT.Amount
                FROM MembershipTransactions MT
                WHERE ISNULL(MT.IsDeleted, 0) = 0
                  AND MT.PaymentStatus = 'Paid'
                  AND CAST(MT.CreatedDate AS DATE) = CAST(@SelectedDate AS DATE)
            ) C";

        public const string RecentMembers =
            @"SELECT TOP 5
                MemberName,
                Status
              FROM MemberMasters
              ORDER BY MemberId DESC";

        public const string MonthlyRevenue =
            @"SELECT
                DATENAME(MONTH, StartDate) AS MonthName,
                SUM(Amount) AS Revenue
              FROM MembershipTransactions
              GROUP BY
                YEAR(StartDate),
                MONTH(StartDate),
                DATENAME(MONTH, StartDate)
              ORDER BY
                YEAR(StartDate),
                MONTH(StartDate)";

        public const string MemberAttendance7Days = @"
        SELECT 
            FORMAT(D.DateVal, 'dd MMM') AS DateLabel,
            COALESCE(COUNT(A.AttendanceId), 0) AS MemberCount
        FROM (
            SELECT CAST(@SelectedDate - 6 AS DATE) AS DateVal UNION ALL
            SELECT CAST(@SelectedDate - 5 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 4 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 3 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 2 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 1 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate AS DATE)
        ) D
        LEFT JOIN Attendances A ON CAST(A.AttendanceDate AS DATE) = D.DateVal
        GROUP BY D.DateVal
        ORDER BY D.DateVal";

        public const string TrainerAttendance7Days = @"
        SELECT 
            FORMAT(D.DateVal, 'dd MMM') AS DateLabel,
            COALESCE(COUNT(SA.AttendanceId), 0) AS TrainerCount
        FROM (
            SELECT CAST(@SelectedDate - 6 AS DATE) AS DateVal UNION ALL
            SELECT CAST(@SelectedDate - 5 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 4 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 3 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 2 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate - 1 AS DATE) UNION ALL
            SELECT CAST(@SelectedDate AS DATE)
        ) D
        LEFT JOIN StaffAttendances SA ON CAST(SA.AttendanceDate AS DATE) = D.DateVal
        LEFT JOIN StaffMasters S ON SA.StaffId = S.StaffId AND S.RoleId = 2
        GROUP BY D.DateVal
        ORDER BY D.DateVal";

        public const string TotalStaff =
            "SELECT COUNT(*) FROM StaffMasters";

        public const string ActiveStaff =
            "SELECT COUNT(*) FROM StaffMasters WHERE IsActive=1";

        public const string StaffAttendanceToday =
            @"SELECT COUNT(*)
              FROM StaffAttendances
              WHERE CAST(AttendanceDate AS DATE) = CAST(@SelectedDate AS DATE)
              AND Status='Present'";

        public const string MonthRevenue = @"
            SELECT ISNULL(SUM(C.Amount), 0)
            FROM (
                SELECT P.Amount FROM Payments P
                WHERE YEAR(P.PaymentDate) = YEAR(@SelectedDate) AND MONTH(P.PaymentDate) = MONTH(@SelectedDate)
                UNION ALL
                SELECT MT.Amount FROM MembershipTransactions MT
                WHERE ISNULL(MT.IsDeleted, 0) = 0 AND MT.PaymentStatus = 'Paid'
                  AND YEAR(MT.CreatedDate) = YEAR(@SelectedDate) AND MONTH(MT.CreatedDate) = MONTH(@SelectedDate)
            ) C";

        public const string OutstandingStats = @"
            SELECT ISNULL(SUM(MT.Amount), 0) AS TotalOutstanding, COUNT(*) AS OutstandingCount
            FROM MembershipTransactions MT
            WHERE ISNULL(MT.IsDeleted, 0) = 0 AND MT.PaymentStatus IN ('Pending', 'Partial')";

        public const string ExpiryStats = @"
            SELECT
                SUM(CASE WHEN M.PlanEndDate < CAST(@SelectedDate AS DATE) THEN 1 ELSE 0 END) AS ExpiredCount,
                SUM(CASE WHEN M.PlanEndDate >= CAST(@SelectedDate AS DATE) THEN 1 ELSE 0 END) AS ExpiringSoonCount
            FROM MemberMasters M
            WHERE M.PlanEndDate IS NOT NULL
              AND M.PlanEndDate <= DATEADD(DAY, 7, CAST(@SelectedDate AS DATE))
              AND M.Status = 'Active'";

        public const string PendingLeads = @"
            SELECT COUNT(*) FROM Leads
            WHERE Status NOT IN ('Converted', 'Lost')";

        public const string NewLeadsToday = @"SELECT COUNT(*) FROM Leads WHERE CAST(CreatedDate AS DATE) = CAST(@SelectedDate AS DATE)";

        public const string FollowUpDue = @"
            SELECT COUNT(*) FROM Leads
            WHERE Status NOT IN ('Converted', 'Lost')
              AND FollowUpDate IS NOT NULL
              AND FollowUpDate <= CAST(@SelectedDate AS DATE)";

        public const string NewMembersThisMonth = @"
            SELECT COUNT(*) FROM MemberMasters
            WHERE YEAR(JoinDate) = YEAR(@SelectedDate) AND MONTH(JoinDate) = MONTH(@SelectedDate)";

        public const string RecentMembersDetailed = @"
            SELECT TOP 5 MemberName, MemberCode, Status, JoinDate FROM MemberMasters ORDER BY MemberId DESC";

        public const string ExpiringMembersTop = @"
            SELECT TOP 5 M.MemberName, P.PlanName, M.PlanEndDate,
                   DATEDIFF(DAY, @SelectedDate, M.PlanEndDate) AS DaysLeft
            FROM MemberMasters M
            LEFT JOIN MembershipPlanMasters P ON M.PlanId = P.PlanId
            WHERE M.PlanEndDate IS NOT NULL
              AND M.PlanEndDate <= DATEADD(DAY, 7, CAST(@SelectedDate AS DATE))
              AND M.Status = 'Active'
            ORDER BY M.PlanEndDate ASC";

        public const string OutstandingTop = @"
            SELECT TOP 5 M.MemberName, MP.PlanName, MT.Amount, MT.PaymentStatus
            FROM MembershipTransactions MT
            INNER JOIN MemberMasters M ON MT.MemberId = M.MemberId
            INNER JOIN MembershipPlanMasters MP ON MT.PlanId = MP.PlanId
            WHERE ISNULL(MT.IsDeleted, 0) = 0 AND MT.PaymentStatus IN ('Pending', 'Partial')
            ORDER BY MT.Amount DESC";

        public const string RecentLeadsTop = @"
            SELECT TOP 5 LeadId, LeadName, MobileNo, Status, CreatedDate
            FROM Leads ORDER BY LeadId DESC";

        public const string FollowUpLeadsTop = @"
            SELECT TOP 5 LeadId, LeadName, MobileNo, Status, FollowUpDate
            FROM Leads
            WHERE Status NOT IN ('Converted', 'Lost')
              AND FollowUpDate IS NOT NULL
              AND FollowUpDate <= CAST(@SelectedDate AS DATE)
            ORDER BY FollowUpDate ASC";

        public const string MonthlyRevenueCombined = @"
            ;WITH Months AS (
                SELECT 0 AS N UNION ALL SELECT 1 UNION ALL SELECT 2
                UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
            ),
            MonthDates AS (
                SELECT DATEADD(MONTH, -N, DATEFROMPARTS(YEAR(@SelectedDate), MONTH(@SelectedDate), 1)) AS MonthStart
                FROM Months
            )
            SELECT FORMAT(M.MonthStart, 'MMM yy') AS MonthLabel,
                   ISNULL(SUM(X.Amount), 0) AS Revenue
            FROM MonthDates M
            LEFT JOIN (
                SELECT CAST(P.PaymentDate AS DATE) AS PayDate, P.Amount FROM Payments P
                UNION ALL
                SELECT CAST(MT.CreatedDate AS DATE), MT.Amount FROM MembershipTransactions MT
                WHERE ISNULL(MT.IsDeleted, 0) = 0 AND MT.PaymentStatus = 'Paid'
            ) X ON YEAR(X.PayDate) = YEAR(M.MonthStart) AND MONTH(X.PayDate) = MONTH(M.MonthStart)
            GROUP BY M.MonthStart, FORMAT(M.MonthStart, 'MMM yy')
            ORDER BY M.MonthStart";

        public const string PaymentModeBreakdown = @"
            SELECT ISNULL(P.PaymentMode, 'Other') AS PaymentMode, SUM(P.Amount) AS TotalAmount
            FROM Payments P
            WHERE P.PaymentDate >= DATEADD(DAY, -30, CAST(@SelectedDate AS DATE))
              AND P.PaymentDate <= CAST(@SelectedDate AS DATE)
            GROUP BY ISNULL(P.PaymentMode, 'Other')
            ORDER BY TotalAmount DESC";
    }
}
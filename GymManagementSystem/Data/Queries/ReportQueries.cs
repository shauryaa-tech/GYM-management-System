namespace GymManagement.Data.Queries
{
    public static class ReportQueries
    {
        public const string AttendanceReport = @"
        SELECT
            A.AttendanceId,
            M.MemberCode,
            M.MemberName,
            A.AttendanceDate,
            A.CheckInTime,
            A.CheckOutTime,
            A.Remarks
        FROM Attendances A
        INNER JOIN MemberMasters M ON A.MemberId = M.MemberId
        WHERE A.AttendanceDate >= @FromDate
          AND A.AttendanceDate <= @ToDate
        ORDER BY A.AttendanceDate DESC, A.AttendanceId DESC";

        public const string AttendanceStats = @"
        SELECT
            COUNT(*) AS TotalRecords,
            COUNT(DISTINCT A.MemberId) AS UniqueMembers
        FROM Attendances A
        WHERE A.AttendanceDate >= @FromDate
          AND A.AttendanceDate <= @ToDate";

        public const string ExpiryReport = @"
        SELECT
            M.MemberId,
            M.MemberCode,
            M.MemberName,
            M.MobileNo,
            P.PlanName,
            M.PlanEndDate,
            DATEDIFF(DAY, GETDATE(), M.PlanEndDate) AS DaysLeft,
            M.Status
        FROM MemberMasters M
        LEFT JOIN MembershipPlanMasters P ON M.PlanId = P.PlanId
        WHERE M.PlanEndDate IS NOT NULL
          AND M.PlanEndDate <= DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE))
          AND M.Status = 'Active'
        ORDER BY M.PlanEndDate ASC";

        public const string ExpiryStats = @"
        SELECT
            SUM(CASE WHEN M.PlanEndDate < CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS ExpiredCount,
            SUM(CASE WHEN M.PlanEndDate >= CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS ExpiringSoonCount
        FROM MemberMasters M
        WHERE M.PlanEndDate IS NOT NULL
          AND M.PlanEndDate <= DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE))
          AND M.Status = 'Active'";

        public const string CollectionsReport = @"
        SELECT
            C.PaymentId,
            C.MemberCode,
            C.MemberName,
            C.PaymentDate,
            C.Amount,
            C.PaymentMode,
            C.ReferenceNo
        FROM (
            SELECT
                P.PaymentId,
                M.MemberCode,
                M.MemberName,
                P.PaymentDate,
                P.Amount,
                ISNULL(P.PaymentMode, 'Other') AS PaymentMode,
                P.ReferenceNo
            FROM Payments P
            INNER JOIN MemberMasters M ON P.MemberId = M.MemberId
            WHERE P.PaymentDate >= @FromDate
              AND P.PaymentDate <= @ToDate

            UNION ALL

            SELECT
                MT.TransactionId + 1000000 AS PaymentId,
                M.MemberCode,
                M.MemberName,
                CAST(MT.CreatedDate AS DATE) AS PaymentDate,
                MT.Amount,
                'Membership' AS PaymentMode,
                CONCAT('MT-', MT.TransactionId) AS ReferenceNo
            FROM MembershipTransactions MT
            INNER JOIN MemberMasters M ON MT.MemberId = M.MemberId
            WHERE ISNULL(MT.IsDeleted, 0) = 0
              AND MT.PaymentStatus = 'Paid'
              AND CAST(MT.CreatedDate AS DATE) >= @FromDate
              AND CAST(MT.CreatedDate AS DATE) <= @ToDate
        ) C
        WHERE 1=1";

        public const string CollectionsStats = @"
        SELECT
            COUNT(*) AS TotalRecords,
            ISNULL(SUM(C.Amount), 0) AS TotalAmount,
            ISNULL(SUM(CASE WHEN C.PaymentMode = 'Cash' THEN C.Amount ELSE 0 END), 0) AS CashTotal,
            ISNULL(SUM(CASE WHEN C.PaymentMode = 'UPI' THEN C.Amount ELSE 0 END), 0) AS UpiTotal,
            ISNULL(SUM(CASE WHEN C.PaymentMode = 'Card' THEN C.Amount ELSE 0 END), 0) AS CardTotal
        FROM (
            SELECT
                P.Amount,
                ISNULL(P.PaymentMode, 'Other') AS PaymentMode
            FROM Payments P
            WHERE P.PaymentDate >= @FromDate
              AND P.PaymentDate <= @ToDate

            UNION ALL

            SELECT
                MT.Amount,
                'Membership' AS PaymentMode
            FROM MembershipTransactions MT
            WHERE ISNULL(MT.IsDeleted, 0) = 0
              AND MT.PaymentStatus = 'Paid'
              AND CAST(MT.CreatedDate AS DATE) >= @FromDate
              AND CAST(MT.CreatedDate AS DATE) <= @ToDate
        ) C
        WHERE 1=1";

        public const string OutstandingReport = @"
        SELECT
            MT.TransactionId,
            M.MemberCode,
            M.MemberName,
            M.MobileNo,
            MP.PlanName,
            MT.Amount,
            MT.PaymentStatus,
            MT.EndDate
        FROM MembershipTransactions MT
        INNER JOIN MemberMasters M ON MT.MemberId = M.MemberId
        INNER JOIN MembershipPlanMasters MP ON MT.PlanId = MP.PlanId
        WHERE ISNULL(MT.IsDeleted, 0) = 0
          AND MT.PaymentStatus IN ('Pending', 'Partial')";

        public const string OutstandingStats = @"
        SELECT
            ISNULL(SUM(MT.Amount), 0) AS TotalOutstanding,
            SUM(CASE WHEN MT.PaymentStatus = 'Pending' THEN 1 ELSE 0 END) AS PendingCount,
            SUM(CASE WHEN MT.PaymentStatus = 'Partial' THEN 1 ELSE 0 END) AS PartialCount
        FROM MembershipTransactions MT
        WHERE ISNULL(MT.IsDeleted, 0) = 0
          AND MT.PaymentStatus IN ('Pending', 'Partial')";

        public const string ProfitLossIncome = @"
        SELECT ISNULL(SUM(C.Amount), 0)
        FROM (
            SELECT P.Amount
            FROM Payments P
            WHERE P.PaymentDate >= @FromDate
              AND P.PaymentDate <= @ToDate

            UNION ALL

            SELECT MT.Amount
            FROM MembershipTransactions MT
            WHERE ISNULL(MT.IsDeleted, 0) = 0
              AND MT.PaymentStatus = 'Paid'
              AND CAST(MT.CreatedDate AS DATE) >= @FromDate
              AND CAST(MT.CreatedDate AS DATE) <= @ToDate
        ) C";

        public const string ProfitLossExpenses = @"
        SELECT ISNULL(SUM(E.Amount), 0)
        FROM Expenses E
        WHERE E.ExpenseDate >= @FromDate
          AND E.ExpenseDate <= @ToDate";

        public const string ProfitLossSalaries = @"
        SELECT ISNULL(SUM(S.NetSalary), 0)
        FROM SalaryProcessings S
        WHERE S.PaidDate IS NOT NULL
          AND S.PaidDate >= @FromDate
          AND S.PaidDate <= @ToDate";

        public const string ProfitLossExpenseBreakdown = @"
        SELECT
            EH.HeadName AS Label,
            ISNULL(SUM(E.Amount), 0) AS Amount
        FROM Expenses E
        INNER JOIN ExpenseHeadMasters EH ON E.ExpenseHeadId = EH.ExpenseHeadId
        WHERE E.ExpenseDate >= @FromDate
          AND E.ExpenseDate <= @ToDate
        GROUP BY EH.HeadName
        ORDER BY Amount DESC";

        public const string ProfitLossIncomeByMode = @"
        SELECT
            C.PaymentMode AS Label,
            ISNULL(SUM(C.Amount), 0) AS Amount
        FROM (
            SELECT
                ISNULL(P.PaymentMode, 'Other') AS PaymentMode,
                P.Amount
            FROM Payments P
            WHERE P.PaymentDate >= @FromDate
              AND P.PaymentDate <= @ToDate

            UNION ALL

            SELECT
                'Membership' AS PaymentMode,
                MT.Amount
            FROM MembershipTransactions MT
            WHERE ISNULL(MT.IsDeleted, 0) = 0
              AND MT.PaymentStatus = 'Paid'
              AND CAST(MT.CreatedDate AS DATE) >= @FromDate
              AND CAST(MT.CreatedDate AS DATE) <= @ToDate
        ) C
        GROUP BY C.PaymentMode
        ORDER BY Amount DESC";
    }
}

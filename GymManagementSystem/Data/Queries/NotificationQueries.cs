namespace GymManagement.Data.Queries
{
    public static class NotificationQueries
    {
        public const string RecentLeads = @"
            SELECT LeadId, LeadName, MobileNo, CreatedDate
            FROM Leads
            WHERE CreatedDate >= DATEADD(HOUR, -@Hours, GETDATE())
            AND Status NOT IN ('Converted', 'Lost')
            ORDER BY CreatedDate DESC";

        public const string RecentPayments = @"
            SELECT P.PaymentId, P.MemberId, M.MemberName, M.MemberCode, P.Amount, P.PaymentDate
            FROM Payments P
            INNER JOIN MemberMasters M ON P.MemberId = M.MemberId
            WHERE P.PaymentDate >= DATEADD(HOUR, -@Hours, GETDATE())
            ORDER BY P.PaymentDate DESC";

        public const string RecentMembers = @"
            SELECT MemberId, MemberName, MemberCode, JoinDate
            FROM MemberMasters
            WHERE JoinDate >= DATEADD(HOUR, -@Hours, GETDATE())
            ORDER BY JoinDate DESC";

        public const string ExpiringMemberships = @"
            SELECT M.MemberId, M.MemberName, M.PlanId, P.PlanName, M.PlanEndDate,
                   DATEDIFF(DAY, GETDATE(), M.PlanEndDate) as DaysLeft
            FROM MemberMasters M
            LEFT JOIN MembershipPlanMasters P ON M.PlanId = P.PlanId
            WHERE M.PlanEndDate BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
            AND M.Status = 'Active'
            ORDER BY M.PlanEndDate ASC";

        public const string CountRecentLeads = @"
            SELECT COUNT(*) FROM Leads
            WHERE CreatedDate >= DATEADD(HOUR, -@Hours, GETDATE())
            AND Status NOT IN ('Converted', 'Lost')";

        public const string CountRecentPayments = @"
            SELECT COUNT(*) FROM Payments
            WHERE PaymentDate >= DATEADD(HOUR, -@Hours, GETDATE())";

        public const string CountRecentMembers = @"
            SELECT COUNT(*) FROM MemberMasters
            WHERE JoinDate >= DATEADD(HOUR, -@Hours, GETDATE())";

        public const string CountExpiringMemberships = @"
            SELECT COUNT(*) FROM MemberMasters
            WHERE PlanEndDate BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
            AND Status = 'Active'";

        public const string FollowUpLeads = @"
            SELECT TOP 10 LeadId, LeadName, MobileNo, FollowUpDate, Status, Remarks
            FROM Leads
            WHERE Status NOT IN ('Converted', 'Lost')
              AND FollowUpDate IS NOT NULL
              AND FollowUpDate <= CAST(GETDATE() AS DATE)
            ORDER BY FollowUpDate ASC";

        public const string CountFollowUpLeads = @"
            SELECT COUNT(*)
            FROM Leads
            WHERE Status NOT IN ('Converted', 'Lost')
              AND FollowUpDate IS NOT NULL
              AND FollowUpDate <= CAST(GETDATE() AS DATE)";
    }
}

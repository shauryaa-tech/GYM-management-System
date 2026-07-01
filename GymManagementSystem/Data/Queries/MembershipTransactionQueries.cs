namespace GymManagementSystem.Data.Queries
{
    public static class MembershipTransactionQueries
    {
        public const string GetAll = @"
        SELECT
            mt.TransactionId,
            mt.MemberId,
            mt.PlanId,
            m.MemberName,
            mp.PlanName,
            mt.StartDate,
            mt.EndDate,
            mt.Amount,
            mt.PaymentStatus,
            mt.MembershipStatus
        FROM MembershipTransactions mt
        INNER JOIN MemberMasters m
            ON mt.MemberId = m.MemberId
        INNER JOIN MembershipPlanMasters mp
            ON mt.PlanId = mp.PlanId
        WHERE ISNULL(mt.IsDeleted,0)=0
        ORDER BY mt.TransactionId DESC";

        public const string Insert = @"
        INSERT INTO MembershipTransactions
        (
            MemberId,
            PlanId,
            StartDate,
            EndDate,
            Amount,
            PaymentStatus,
            MembershipStatus,
            Remarks,
            CreatedDate,
            IsDeleted
        )
        VALUES
        (
            @MemberId,
            @PlanId,
            @StartDate,
            @EndDate,
            @Amount,
            @PaymentStatus,
            @MembershipStatus,
            @Remarks,
            GETDATE(),
            0
        )";

        public const string Delete = @"
        UPDATE MembershipTransactions
        SET IsDeleted = 1
        WHERE TransactionId=@TransactionId";

        public const string GetById = @"
        SELECT
            mt.TransactionId,
            mt.MemberId,
            mt.PlanId,
            m.MemberName,
            mp.PlanName,
            mt.StartDate,
            mt.EndDate,
            mt.Amount,
            mt.PaymentStatus,
            mt.MembershipStatus,
            mt.Remarks
        FROM MembershipTransactions mt
        INNER JOIN MemberMasters m ON mt.MemberId = m.MemberId
        INNER JOIN MembershipPlanMasters mp ON mt.PlanId = mp.PlanId
        WHERE mt.TransactionId = @TransactionId
          AND ISNULL(mt.IsDeleted, 0) = 0";

        public const string UpdatePaymentStatus = @"
        UPDATE MembershipTransactions
        SET PaymentStatus = @PaymentStatus,
            Remarks = CASE
                WHEN @Remarks IS NULL OR LTRIM(RTRIM(@Remarks)) = '' THEN Remarks
                ELSE @Remarks
            END
        WHERE TransactionId = @TransactionId
          AND ISNULL(IsDeleted, 0) = 0";
    }
}
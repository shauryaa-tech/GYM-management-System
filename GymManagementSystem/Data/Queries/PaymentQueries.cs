namespace GymManagement.Data.Queries
{
    public static class PaymentQueries
    {
        public const string GetAll = @"
        SELECT 
            P.*,
            M.MemberName,
            M.MemberCode
        FROM Payments P
        INNER JOIN MemberMasters M ON P.MemberId = M.MemberId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM Payments
        WHERE PaymentId=@PaymentId";

        public const string Insert = @"
        INSERT INTO Payments
        (
            MemberId,
            PaymentDate,
            Amount,
            PaymentMode,
            ReferenceNo,
            Remarks
        )
        VALUES
        (
            @MemberId,
            @PaymentDate,
            @Amount,
            @PaymentMode,
            @ReferenceNo,
            @Remarks
        )";

        public const string Update = @"
        UPDATE Payments
        SET
            MemberId=@MemberId,
            PaymentDate=@PaymentDate,
            Amount=@Amount,
            PaymentMode=@PaymentMode,
            ReferenceNo=@ReferenceNo,
            Remarks=@Remarks
        WHERE PaymentId=@PaymentId";

        public const string Delete =
            "DELETE FROM Payments WHERE PaymentId=@PaymentId";
    }
}

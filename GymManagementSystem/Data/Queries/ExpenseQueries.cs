namespace GymManagement.Data.Queries
{
    public static class ExpenseQueries
    {
        public const string GetAll = @"
        SELECT
            E.*,
            EH.HeadName
        FROM Expenses E
        LEFT JOIN ExpenseHeadMasters EH ON E.ExpenseHeadId = EH.ExpenseHeadId
        WHERE 1=1";

        public const string GetById = @"
        SELECT
            E.*,
            EH.HeadName
        FROM Expenses E
        LEFT JOIN ExpenseHeadMasters EH ON E.ExpenseHeadId = EH.ExpenseHeadId
        WHERE E.ExpenseId=@ExpenseId";

        public const string Insert = @"
        INSERT INTO Expenses
        (
            ExpenseHeadId,
            Amount,
            ExpenseDate,
            Description,
            PaymentMode,
            PaidTo,
            Remarks
        )
        VALUES
        (
            @ExpenseHeadId,
            @Amount,
            @ExpenseDate,
            @Description,
            @PaymentMode,
            @PaidTo,
            @Remarks
        )";

        public const string Update = @"
        UPDATE Expenses
        SET
            ExpenseHeadId=@ExpenseHeadId,
            Amount=@Amount,
            ExpenseDate=@ExpenseDate,
            Description=@Description,
            PaymentMode=@PaymentMode,
            PaidTo=@PaidTo,
            Remarks=@Remarks
        WHERE ExpenseId=@ExpenseId";

        public const string Delete =
            "DELETE FROM Expenses WHERE ExpenseId=@ExpenseId";

        public const string GetActiveExpenseHeads = @"
        SELECT ExpenseHeadId, HeadName
        FROM ExpenseHeadMasters
        WHERE IsActive=1
        ORDER BY HeadName";
    }
}

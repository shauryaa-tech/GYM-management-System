namespace GymManagement.Data.Queries
{
    public static class StockIssueQueries
    {
        public const string GetAll = @"
        SELECT 
            SI.*,
            P.ProductName,
            P.Category,
            P.CurrentStock,
            M.MemberName,
            M.MemberCode
        FROM StockIssues SI
        LEFT JOIN ProductMasters P ON SI.ProductId = P.ProductId
        LEFT JOIN MemberMasters M ON SI.MemberId = M.MemberId
        WHERE 1=1";

        public const string GetById = @"
        SELECT 
            SI.*,
            P.ProductName,
            P.Category,
            P.CurrentStock,
            M.MemberName,
            M.MemberCode
        FROM StockIssues SI
        LEFT JOIN ProductMasters P ON SI.ProductId = P.ProductId
        LEFT JOIN MemberMasters M ON SI.MemberId = M.MemberId
        WHERE SI.IssueId=@IssueId";

        public const string Insert = @"
        INSERT INTO StockIssues
        (
            ProductId,
            MemberId,
            Quantity,
            IssueDate,
            IssuedTo,
            Amount,
            PaymentMode,
            Remarks
        )
        VALUES
        (
            @ProductId,
            @MemberId,
            @Quantity,
            @IssueDate,
            @IssuedTo,
            @Amount,
            @PaymentMode,
            @Remarks
        )";

        public const string Update = @"
        UPDATE StockIssues
        SET
            ProductId=@ProductId,
            MemberId=@MemberId,
            Quantity=@Quantity,
            IssueDate=@IssueDate,
            IssuedTo=@IssuedTo,
            Amount=@Amount,
            PaymentMode=@PaymentMode,
            Remarks=@Remarks
        WHERE IssueId=@IssueId";

        public const string Delete = "DELETE FROM StockIssues WHERE IssueId=@IssueId";

        public const string GetActiveProducts = @"
        SELECT ProductId, ProductName, Category, UnitPrice, CurrentStock
        FROM ProductMasters
        WHERE IsActive=1";

        public const string GetActiveMembers = @"
        SELECT MemberId, MemberCode, MemberName
        FROM MemberMasters
        WHERE Status='Active'
        ORDER BY MemberName";

        public const string UpdateProductStock = @"
        UPDATE ProductMasters
        SET CurrentStock = CurrentStock - @Quantity
        WHERE ProductId = @ProductId";
    }
}

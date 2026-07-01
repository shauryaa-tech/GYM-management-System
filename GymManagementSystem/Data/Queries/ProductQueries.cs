namespace GymManagement.Data.Queries
{
    public static class ProductQueries
    {
        public const string GetAll = @"
        SELECT 
            P.*,
            V.VendorName
        FROM ProductMasters P
        LEFT JOIN VendorMasters V ON P.VendorId = V.VendorId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM ProductMasters
        WHERE ProductId=@ProductId";

        public const string Insert = @"
        INSERT INTO ProductMasters
        (
            ProductName,
            Category,
            UnitPrice,
            CurrentStock,
            ReorderLevel,
            VendorId,
            IsActive
        )
        VALUES
        (
            @ProductName,
            @Category,
            @UnitPrice,
            @CurrentStock,
            @ReorderLevel,
            @VendorId,
            @IsActive
        )";

        public const string Update = @"
        UPDATE ProductMasters
        SET
            ProductName=@ProductName,
            Category=@Category,
            UnitPrice=@UnitPrice,
            CurrentStock=@CurrentStock,
            ReorderLevel=@ReorderLevel,
            VendorId=@VendorId,
            IsActive=@IsActive
        WHERE ProductId=@ProductId";

        public const string Delete =
            "DELETE FROM ProductMasters WHERE ProductId=@ProductId";
    }
}

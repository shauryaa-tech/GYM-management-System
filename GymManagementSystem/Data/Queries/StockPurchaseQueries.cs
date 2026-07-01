namespace GymManagement.Data.Queries
{
    public static class StockPurchaseQueries
    {
        public const string GetAll = @"
        SELECT 
            SP.*,
            P.ProductName,
            P.Category,
            P.CurrentStock,
            V.VendorName
        FROM StockPurchases SP
        LEFT JOIN ProductMasters P ON SP.ProductId = P.ProductId
        LEFT JOIN VendorMasters V ON SP.VendorId = V.VendorId
        WHERE 1=1";

        public const string GetById = @"
        SELECT 
            SP.*,
            P.ProductName,
            P.Category,
            P.CurrentStock,
            V.VendorName
        FROM StockPurchases SP
        LEFT JOIN ProductMasters P ON SP.ProductId = P.ProductId
        LEFT JOIN VendorMasters V ON SP.VendorId = V.VendorId
        WHERE SP.PurchaseId=@PurchaseId";

        public const string Insert = @"
        INSERT INTO StockPurchases
        (
            ProductId,
            VendorId,
            Quantity,
            UnitPrice,
            TotalAmount,
            PurchaseDate,
            InvoiceNo,
            Remarks
        )
        VALUES
        (
            @ProductId,
            @VendorId,
            @Quantity,
            @UnitPrice,
            @TotalAmount,
            @PurchaseDate,
            @InvoiceNo,
            @Remarks
        )";

        public const string Update = @"
        UPDATE StockPurchases
        SET
            ProductId=@ProductId,
            VendorId=@VendorId,
            Quantity=@Quantity,
            UnitPrice=@UnitPrice,
            TotalAmount=@TotalAmount,
            PurchaseDate=@PurchaseDate,
            InvoiceNo=@InvoiceNo,
            Remarks=@Remarks
        WHERE PurchaseId=@PurchaseId";

        public const string Delete = "DELETE FROM StockPurchases WHERE PurchaseId=@PurchaseId";

        public const string GetActiveProducts = @"
        SELECT ProductId, ProductName, Category, UnitPrice, CurrentStock, VendorId
        FROM ProductMasters
        WHERE IsActive=1";

        public const string GetActiveVendors = @"
        SELECT VendorId, VendorName
        FROM VendorMasters
        WHERE IsActive=1";

        public const string UpdateProductStock = @"
        UPDATE ProductMasters
        SET CurrentStock = CurrentStock + @Quantity
        WHERE ProductId = @ProductId";
    }
}
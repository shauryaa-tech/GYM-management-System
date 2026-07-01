namespace GymManagement.Data.Queries
{
    public static class EquipmentQueries
    {
        public const string GetAll = @"
        SELECT * FROM EquipmentMasters
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM EquipmentMasters
        WHERE EquipmentId=@EquipmentId";

        public const string Insert = @"
        INSERT INTO EquipmentMasters
        (
            EquipmentName,
            Category,
            PurchaseDate,
            PurchasePrice,
            Quantity,
            ConditionStatus,
            Location,
            Remarks,
            IsActive
        )
        VALUES
        (
            @EquipmentName,
            @Category,
            @PurchaseDate,
            @PurchasePrice,
            @Quantity,
            @ConditionStatus,
            @Location,
            @Remarks,
            @IsActive
        )";

        public const string Update = @"
        UPDATE EquipmentMasters
        SET
            EquipmentName=@EquipmentName,
            Category=@Category,
            PurchaseDate=@PurchaseDate,
            PurchasePrice=@PurchasePrice,
            Quantity=@Quantity,
            ConditionStatus=@ConditionStatus,
            Location=@Location,
            Remarks=@Remarks,
            IsActive=@IsActive
        WHERE EquipmentId=@EquipmentId";

        public const string Delete =
            "DELETE FROM EquipmentMasters WHERE EquipmentId=@EquipmentId";
    }
}

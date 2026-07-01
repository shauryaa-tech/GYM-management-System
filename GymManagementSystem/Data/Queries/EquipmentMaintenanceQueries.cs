namespace GymManagement.Data.Queries
{
    public static class EquipmentMaintenanceQueries
    {
        public const string GetAll = @"
        SELECT 
            EM.*,
            E.EquipmentName,
            E.Category,
            E.Location
        FROM EquipmentMaintenances EM
        LEFT JOIN EquipmentMasters E ON EM.EquipmentId = E.EquipmentId
        WHERE 1=1";

        public const string GetById = @"
        SELECT 
            EM.*,
            E.EquipmentName,
            E.Category,
            E.Location
        FROM EquipmentMaintenances EM
        LEFT JOIN EquipmentMasters E ON EM.EquipmentId = E.EquipmentId
        WHERE EM.MaintenanceId=@MaintenanceId";

        public const string Insert = @"
        INSERT INTO EquipmentMaintenances
        (
            EquipmentId,
            MaintenanceDate,
            MaintenanceType,
            Cost,
            PaymentMode,
            VendorName,
            NextDueDate,
            Status,
            Remarks
        )
        VALUES
        (
            @EquipmentId,
            @MaintenanceDate,
            @MaintenanceType,
            @Cost,
            @PaymentMode,
            @VendorName,
            @NextDueDate,
            @Status,
            @Remarks
        )";

        public const string Update = @"
        UPDATE EquipmentMaintenances
        SET
            EquipmentId=@EquipmentId,
            MaintenanceDate=@MaintenanceDate,
            MaintenanceType=@MaintenanceType,
            Cost=@Cost,
            PaymentMode=@PaymentMode,
            VendorName=@VendorName,
            NextDueDate=@NextDueDate,
            Status=@Status,
            Remarks=@Remarks
        WHERE MaintenanceId=@MaintenanceId";

        public const string Delete = "DELETE FROM EquipmentMaintenances WHERE MaintenanceId=@MaintenanceId";

        public const string GetActiveEquipment = @"
        SELECT EquipmentId, EquipmentName, Category, Location
        FROM EquipmentMasters
        WHERE IsActive=1
        ORDER BY EquipmentName";
    }
}

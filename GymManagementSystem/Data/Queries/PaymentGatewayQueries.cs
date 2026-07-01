namespace GymManagement.Data.Queries
{
    public static class PaymentGatewayQueries
    {
        public const string BaseSelect = @"
        SELECT *
        FROM PaymentGateways
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM PaymentGateways WHERE Id = @Id";

        public const string GetDefaultActive = @"
        SELECT TOP 1 * FROM PaymentGateways
        WHERE IsDefault = 1 AND IsActive = 1 AND IsValidated = 1";

        public const string GetByGatewayName = @"
        SELECT TOP 1 * FROM PaymentGateways
        WHERE GatewayName = @GatewayName AND IsActive = 1";

        public const string Insert = @"
        INSERT INTO PaymentGateways
        (
            GatewayName, DisplayName, MerchantId, MerchantKey, MID, ChannelId,
            Website, IndustryType, CallbackUrl, Environment, SandboxBaseUrl,
            ProductionBaseUrl, IsDefault, IsActive, IsValidated, ValidationMessage,
            LastValidatedOn, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
        )
        VALUES
        (
            @GatewayName, @DisplayName, @MerchantId, @MerchantKey, @MID, @ChannelId,
            @Website, @IndustryType, @CallbackUrl, @Environment, @SandboxBaseUrl,
            @ProductionBaseUrl, @IsDefault, @IsActive, @IsValidated, @ValidationMessage,
            @LastValidatedOn, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate
        )";

        public const string Update = @"
        UPDATE PaymentGateways SET
            GatewayName = @GatewayName,
            DisplayName = @DisplayName,
            MerchantId = @MerchantId,
            MerchantKey = @MerchantKey,
            MID = @MID,
            ChannelId = @ChannelId,
            Website = @Website,
            IndustryType = @IndustryType,
            CallbackUrl = @CallbackUrl,
            Environment = @Environment,
            SandboxBaseUrl = @SandboxBaseUrl,
            ProductionBaseUrl = @ProductionBaseUrl,
            IsDefault = @IsDefault,
            IsActive = @IsActive,
            IsValidated = @IsValidated,
            ValidationMessage = @ValidationMessage,
            LastValidatedOn = @LastValidatedOn,
            ModifiedBy = @ModifiedBy,
            ModifiedDate = @ModifiedDate
        WHERE Id = @Id";

        public const string Delete = @"DELETE FROM PaymentGateways WHERE Id = @Id";

        public const string ClearDefault = @"
        UPDATE PaymentGateways SET IsDefault = 0
        WHERE IsDefault = 1 AND (@ExceptId IS NULL OR Id <> @ExceptId)";
    }
}

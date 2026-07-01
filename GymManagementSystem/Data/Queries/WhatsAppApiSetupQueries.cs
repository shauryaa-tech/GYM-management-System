namespace GymManagement.Data.Queries
{
    public static class WhatsAppApiSetupQueries
    {
        public const string Get = @"
            SELECT TOP 1 *
            FROM WhatsAppApiSettings
            WHERE Id = 1";

        public const string Upsert = @"
            IF EXISTS (SELECT 1 FROM WhatsAppApiSettings WHERE Id = 1)
            BEGIN
                UPDATE WhatsAppApiSettings
                SET IsEnabled = @IsEnabled,
                    ApiProvider = @ApiProvider,
                    ApiBaseUrl = @ApiBaseUrl,
                    PhoneNumberId = @PhoneNumberId,
                    BusinessPhone = @BusinessPhone,
                    WabaId = @WabaId,
                    AppId = @AppId,
                    AccessToken = @AccessToken,
                    VerifyToken = @VerifyToken,
                    GraphApiVersion = @GraphApiVersion,
                    WelcomeMessage = @WelcomeMessage,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = GETDATE()
                WHERE Id = 1;
            END
            ELSE
            BEGIN
                INSERT INTO WhatsAppApiSettings
                (Id, IsEnabled, ApiProvider, ApiBaseUrl, PhoneNumberId, BusinessPhone, WabaId, AppId, AccessToken, VerifyToken, GraphApiVersion, WelcomeMessage, ModifiedBy, ModifiedDate)
                VALUES
                (1, @IsEnabled, @ApiProvider, @ApiBaseUrl, @PhoneNumberId, @BusinessPhone, @WabaId, @AppId, @AccessToken, @VerifyToken, @GraphApiVersion, @WelcomeMessage, @ModifiedBy, GETDATE());
            END";
    }
}

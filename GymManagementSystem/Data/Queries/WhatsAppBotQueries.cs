namespace GymManagement.Data.Queries
{
    public static class WhatsAppBotQueries
    {
        public const string InsertSession = @"
            INSERT INTO WhatsAppBotSessions
            (LeadId, PhoneNumber, CurrentStep, PaymentToken, IsCompleted, CreatedDate)
            VALUES
            (@LeadId, @PhoneNumber, @CurrentStep, @PaymentToken, 0, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public const string GetActiveByPhone = @"
            SELECT TOP 1 S.*, L.LeadName
            FROM WhatsAppBotSessions S
            INNER JOIN Leads L ON S.LeadId = L.LeadId
            WHERE S.PhoneNumber = @PhoneNumber
              AND S.IsCompleted = 0
            ORDER BY S.SessionId DESC";

        public const string GetByPaymentToken = @"
            SELECT S.*, L.LeadName
            FROM WhatsAppBotSessions S
            INNER JOIN Leads L ON S.LeadId = L.LeadId
            WHERE S.PaymentToken = @PaymentToken";

        public const string GetByLeadId = @"
            SELECT TOP 1 S.*, L.LeadName
            FROM WhatsAppBotSessions S
            INNER JOIN Leads L ON S.LeadId = L.LeadId
            WHERE S.LeadId = @LeadId
            ORDER BY S.SessionId DESC";

        public const string UpdateSession = @"
            UPDATE WhatsAppBotSessions
            SET CurrentStep = @CurrentStep,
                SelectedTrainerId = @SelectedTrainerId,
                SelectedClassId = @SelectedClassId,
                SelectedPlanId = @SelectedPlanId,
                PaymentToken = @PaymentToken,
                IsCompleted = @IsCompleted,
                UpdatedDate = GETDATE()
            WHERE SessionId = @SessionId";

        public const string GetLeadSourceIdByName = @"
            SELECT TOP 1 LeadSourceId
            FROM LeadSourceMasters
            WHERE SourceName = @SourceName AND IsActive = 1";
    }
}

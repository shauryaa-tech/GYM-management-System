-- WhatsApp Bot module for CloudMex Gym
-- Run after Schema_Updates.sql

IF NOT EXISTS (SELECT 1 FROM LeadSourceMasters WHERE SourceName = 'WhatsApp Bot')
BEGIN
    INSERT INTO LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES ('LSWA', 'WhatsApp Bot', 'Leads from WhatsApp bot / public join form', 99, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM LeadSourceMasters WHERE SourceName = 'Public Form')
BEGIN
    INSERT INTO LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES ('LSPF', 'Public Form', 'Leads from public join form link', 98, 1);
END
GO

IF OBJECT_ID('WhatsAppBotSessions', 'U') IS NULL
BEGIN
    CREATE TABLE WhatsAppBotSessions (
        SessionId INT IDENTITY(1,1) PRIMARY KEY,
        LeadId INT NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        CurrentStep NVARCHAR(50) NOT NULL DEFAULT 'SelectTrainer',
        SelectedTrainerId INT NULL,
        SelectedClassId INT NULL,
        SelectedPlanId INT NULL,
        PaymentToken NVARCHAR(100) NULL,
        IsCompleted BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CONSTRAINT FK_WhatsAppBotSessions_Leads FOREIGN KEY (LeadId) REFERENCES Leads(LeadId)
    );

    CREATE INDEX IX_WhatsAppBotSessions_Phone ON WhatsAppBotSessions(PhoneNumber);
    CREATE INDEX IX_WhatsAppBotSessions_Lead ON WhatsAppBotSessions(LeadId);
    CREATE INDEX IX_WhatsAppBotSessions_PaymentToken ON WhatsAppBotSessions(PaymentToken);
END
GO

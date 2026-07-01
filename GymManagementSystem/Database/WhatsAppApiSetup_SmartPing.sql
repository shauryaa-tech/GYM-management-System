-- SmartPing provider columns for WhatsApp API Setup

IF OBJECT_ID('WhatsAppApiSettings', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('WhatsAppApiSettings', 'ApiProvider') IS NULL
        ALTER TABLE WhatsAppApiSettings ADD ApiProvider NVARCHAR(50) NOT NULL
            CONSTRAINT DF_WA_ApiProvider DEFAULT 'SmartPing';

    IF COL_LENGTH('WhatsAppApiSettings', 'ApiBaseUrl') IS NULL
        ALTER TABLE WhatsAppApiSettings ADD ApiBaseUrl NVARCHAR(500) NULL;

    UPDATE WhatsAppApiSettings
    SET ApiProvider = 'SmartPing'
    WHERE ApiProvider IS NULL OR LTRIM(RTRIM(ApiProvider)) = '';
END
GO

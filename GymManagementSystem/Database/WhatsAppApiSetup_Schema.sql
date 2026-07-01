-- WhatsApp API Setup Master + permissions

IF OBJECT_ID('WhatsAppApiSettings', 'U') IS NULL
BEGIN
    CREATE TABLE WhatsAppApiSettings (
        Id INT NOT NULL CONSTRAINT PK_WhatsAppApiSettings PRIMARY KEY DEFAULT 1,
        IsEnabled BIT NOT NULL DEFAULT 0,
        PhoneNumberId NVARCHAR(100) NULL,
        BusinessPhone NVARCHAR(20) NULL,
        WabaId NVARCHAR(100) NULL,
        AppId NVARCHAR(100) NULL,
        AccessToken NVARCHAR(1000) NULL,
        VerifyToken NVARCHAR(200) NULL,
        GraphApiVersion NVARCHAR(20) NOT NULL DEFAULT 'v21.0',
        WelcomeMessage NVARCHAR(500) NULL,
        ModifiedBy INT NULL,
        ModifiedDate DATETIME NULL,
        CONSTRAINT CK_WhatsAppApiSettings_SingleRow CHECK (Id = 1)
    );

    INSERT INTO WhatsAppApiSettings (Id, IsEnabled, VerifyToken, GraphApiVersion, WelcomeMessage)
    VALUES (1, 0, 'cloudmex-verify-token', 'v21.0', 'Welcome to CloudMex Gym!');
END
GO

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'WhatsAppApiSetup')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('WhatsAppApiSetup', 'WhatsApp API Setup', 95, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'WhatsAppBot')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('WhatsAppBot', 'WhatsApp Bot', 96, 1);
END
GO

INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT 1, P.PermissionId, 1, 1, 1, 0, 0
FROM PermissionMaster P
WHERE P.ModuleName IN ('WhatsAppApiSetup', 'WhatsAppBot')
AND NOT EXISTS (
    SELECT 1 FROM RolePermission RP
    WHERE RP.RoleId = 1 AND RP.PermissionId = P.PermissionId
);
GO

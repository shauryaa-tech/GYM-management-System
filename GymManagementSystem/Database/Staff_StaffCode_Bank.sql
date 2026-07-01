-- Staff unique code (biometric) + bank details for salary transfer

IF COL_LENGTH('StaffMasters', 'StaffCode') IS NULL
    ALTER TABLE StaffMasters ADD StaffCode NVARCHAR(20) NULL;
IF COL_LENGTH('StaffMasters', 'BankName') IS NULL
    ALTER TABLE StaffMasters ADD BankName NVARCHAR(150) NULL;
IF COL_LENGTH('StaffMasters', 'BankAccountNo') IS NULL
    ALTER TABLE StaffMasters ADD BankAccountNo NVARCHAR(50) NULL;
IF COL_LENGTH('StaffMasters', 'IfscCode') IS NULL
    ALTER TABLE StaffMasters ADD IfscCode NVARCHAR(20) NULL;
GO

UPDATE StaffMasters
SET StaffCode = 'EMP' + RIGHT('000' + CAST(StaffId AS VARCHAR(10)), 3)
WHERE StaffCode IS NULL OR LTRIM(RTRIM(StaffCode)) = '';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_StaffMasters_StaffCode' AND object_id = OBJECT_ID('StaffMasters'))
BEGIN
    CREATE UNIQUE INDEX UX_StaffMasters_StaffCode ON StaffMasters(StaffCode)
    WHERE StaffCode IS NOT NULL AND LTRIM(RTRIM(StaffCode)) <> '';
END
GO

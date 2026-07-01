-- Leads table — optional columns for full CRM features
-- Safe to run multiple times (checks COL_LENGTH before ALTER)

IF COL_LENGTH('Leads', 'LeadCode') IS NULL
    ALTER TABLE Leads ADD LeadCode NVARCHAR(20) NULL;

IF COL_LENGTH('Leads', 'AlternateMobile') IS NULL
    ALTER TABLE Leads ADD AlternateMobile NVARCHAR(20) NULL;

IF COL_LENGTH('Leads', 'Gender') IS NULL
    ALTER TABLE Leads ADD Gender NVARCHAR(20) NULL;

IF COL_LENGTH('Leads', 'Address') IS NULL
    ALTER TABLE Leads ADD Address NVARCHAR(500) NULL;

IF COL_LENGTH('Leads', 'Budget') IS NULL
    ALTER TABLE Leads ADD Budget DECIMAL(18,2) NULL;

IF COL_LENGTH('Leads', 'IsConverted') IS NULL
    ALTER TABLE Leads ADD IsConverted BIT NOT NULL DEFAULT 0;

IF COL_LENGTH('Leads', 'IsActive') IS NULL
    ALTER TABLE Leads ADD IsActive BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('Leads', 'ModifiedDate') IS NULL
    ALTER TABLE Leads ADD ModifiedDate DATETIME NULL;
GO

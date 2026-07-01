-- Payment Gateway Module Tables

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentGateways')
BEGIN
    CREATE TABLE PaymentGateways (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        GatewayName NVARCHAR(50) NOT NULL,
        DisplayName NVARCHAR(100) NOT NULL,
        MerchantId NVARCHAR(100) NULL,
        MerchantKey NVARCHAR(500) NULL,
        MID NVARCHAR(100) NULL,
        ChannelId NVARCHAR(50) NULL,
        Website NVARCHAR(50) NULL,
        IndustryType NVARCHAR(50) NULL,
        CallbackUrl NVARCHAR(500) NULL,
        Environment NVARCHAR(20) NOT NULL DEFAULT 'Sandbox',
        SandboxBaseUrl NVARCHAR(500) NULL,
        ProductionBaseUrl NVARCHAR(500) NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        IsValidated BIT NOT NULL DEFAULT 0,
        ValidationMessage NVARCHAR(1000) NULL,
        LastValidatedOn DATETIME2 NULL,
        CreatedBy INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy INT NULL,
        ModifiedDate DATETIME2 NULL
    );

    CREATE INDEX IX_PaymentGateways_GatewayName ON PaymentGateways(GatewayName);
    CREATE INDEX IX_PaymentGateways_IsDefault ON PaymentGateways(IsDefault);
    CREATE INDEX IX_PaymentGateways_IsActive ON PaymentGateways(IsActive);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentTransactions')
BEGIN
    CREATE TABLE PaymentTransactions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NULL,
        OrderId NVARCHAR(100) NOT NULL,
        TransactionId NVARCHAR(100) NULL,
        Gateway NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL DEFAULT 'INR',
        PaymentFor NVARCHAR(100) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        ResponseCode NVARCHAR(20) NULL,
        ResponseMessage NVARCHAR(500) NULL,
        GatewayResponse NVARCHAR(MAX) NULL,
        PaidOn DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE UNIQUE INDEX IX_PaymentTransactions_OrderId ON PaymentTransactions(OrderId);
    CREATE INDEX IX_PaymentTransactions_TransactionId ON PaymentTransactions(TransactionId);
    CREATE INDEX IX_PaymentTransactions_MemberId ON PaymentTransactions(MemberId);
    CREATE INDEX IX_PaymentTransactions_Status ON PaymentTransactions(Status);
END
GO

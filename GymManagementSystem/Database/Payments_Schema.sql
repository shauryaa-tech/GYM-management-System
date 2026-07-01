-- Payments table for member payment collections (used by Payments module and Reports)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments')
BEGIN
    CREATE TABLE Payments (
        PaymentId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        PaymentDate DATE NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        PaymentMode NVARCHAR(50) NULL,
        ReferenceNo NVARCHAR(100) NULL,
        Remarks NVARCHAR(500) NULL,
        CONSTRAINT FK_Payments_MemberMasters FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId)
    );
END

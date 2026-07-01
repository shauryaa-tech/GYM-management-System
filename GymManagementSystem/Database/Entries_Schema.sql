-- Entry tables for Gym Management System

-- 1. StockIssues (Depends on ProductMasters, MemberMasters)
CREATE TABLE StockIssues (
    IssueId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    MemberId INT NULL,
    Quantity INT NOT NULL,
    IssueDate DATE NOT NULL,
    IssuedTo NVARCHAR(200) NOT NULL,
    Amount DECIMAL(18,2) NULL,
    PaymentMode NVARCHAR(50),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (ProductId) REFERENCES ProductMasters(ProductId),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId)
);

-- 2. EquipmentMaintenances (Depends on EquipmentMasters)
CREATE TABLE EquipmentMaintenances (
    MaintenanceId INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentId INT NOT NULL,
    MaintenanceDate DATE NOT NULL,
    MaintenanceType NVARCHAR(100) NOT NULL,
    Cost DECIMAL(18,2) NOT NULL,
    PaymentMode NVARCHAR(50),
    VendorName NVARCHAR(200),
    NextDueDate DATE NULL,
    Status NVARCHAR(50) DEFAULT 'Completed',
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (EquipmentId) REFERENCES EquipmentMasters(EquipmentId)
);

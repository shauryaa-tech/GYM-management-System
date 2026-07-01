/*
================================================================================
  CloudMex Gym Management System — FULL DATABASE SCRIPT (Single File)
================================================================================
  Database : GymManagement
  Usage    : SSMS mein poori script run karein (fresh ya existing DB par safe)

  Features :
  - Sab tables ek jagah (base + modules + payroll + WhatsApp + payments)
  - IF NOT EXISTS / COL_LENGTH checks — dobara run kar sakte hain
  - Correct table names: RoleMaster, ExerciseMaster (app code ke mutabiq)

  Admin user :
  - BCrypt hash generate karke UserMasters mein insert karein (README dekhein)
  - RoleId = 1 = Super Admin (sab permissions)
================================================================================
*/

-- ========== STEP 0: Database ==========
IF DB_ID(N'GymManagement') IS NULL
BEGIN
    CREATE DATABASE GymManagement;
END
GO

USE GymManagement;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================================================
-- SECTION 1: CORE / BASE TABLES
-- =============================================================================

-- 1.1 RoleMaster
IF OBJECT_ID(N'dbo.RoleMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoleMaster (
        RoleId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RoleMaster PRIMARY KEY,
        RoleName        NVARCHAR(100) NOT NULL,
        Description     NVARCHAR(500) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_RoleMaster_IsActive DEFAULT (1),
        CreatedDate     DATETIME NOT NULL CONSTRAINT DF_RoleMaster_CreatedDate DEFAULT (GETDATE())
    );
END
GO

-- 1.2 PermissionMaster
IF OBJECT_ID(N'dbo.PermissionMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PermissionMaster (
        PermissionId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PermissionMaster PRIMARY KEY,
        ModuleName      NVARCHAR(100) NOT NULL,
        DisplayName     NVARCHAR(200) NOT NULL,
        SortOrder       INT NOT NULL CONSTRAINT DF_PermissionMaster_SortOrder DEFAULT (0),
        IsActive        BIT NOT NULL CONSTRAINT DF_PermissionMaster_IsActive DEFAULT (1)
    );
    CREATE UNIQUE INDEX UX_PermissionMaster_ModuleName ON dbo.PermissionMaster(ModuleName);
END
GO

-- 1.3 RolePermission
IF OBJECT_ID(N'dbo.RolePermission', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermission (
        RolePermissionId  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolePermission PRIMARY KEY,
        RoleId            INT NOT NULL,
        PermissionId      INT NOT NULL,
        CanView           BIT NOT NULL CONSTRAINT DF_RolePermission_CanView DEFAULT (0),
        CanAdd            BIT NOT NULL CONSTRAINT DF_RolePermission_CanAdd DEFAULT (0),
        CanEdit           BIT NOT NULL CONSTRAINT DF_RolePermission_CanEdit DEFAULT (0),
        CanDelete         BIT NOT NULL CONSTRAINT DF_RolePermission_CanDelete DEFAULT (0),
        CanExport         BIT NOT NULL CONSTRAINT DF_RolePermission_CanExport DEFAULT (0),
        CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleId) REFERENCES dbo.RoleMaster(RoleId),
        CONSTRAINT FK_RolePermission_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.PermissionMaster(PermissionId),
        CONSTRAINT UX_RolePermission_Role_Permission UNIQUE (RoleId, PermissionId)
    );
END
GO

-- 1.4 MembershipPlanMasters
IF OBJECT_ID(N'dbo.MembershipPlanMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MembershipPlanMasters (
        PlanId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MembershipPlanMasters PRIMARY KEY,
        PlanName        NVARCHAR(200) NOT NULL,
        DurationMonths  INT NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        JoiningFee      DECIMAL(18,2) NOT NULL CONSTRAINT DF_MembershipPlanMasters_JoiningFee DEFAULT (0),
        Description     NVARCHAR(MAX) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_MembershipPlanMasters_IsActive DEFAULT (1)
    );
END
GO

-- 1.5 ExerciseMaster
IF OBJECT_ID(N'dbo.ExerciseMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExerciseMaster (
        ExerciseId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExerciseMaster PRIMARY KEY,
        ExerciseName    NVARCHAR(200) NOT NULL,
        MuscleGroup     NVARCHAR(100) NOT NULL,
        DifficultyLevel NVARCHAR(50) NOT NULL,
        CaloriesBurn    INT NOT NULL CONSTRAINT DF_ExerciseMaster_CaloriesBurn DEFAULT (0),
        Description     NVARCHAR(MAX) NOT NULL CONSTRAINT DF_ExerciseMaster_Description DEFAULT (N''),
        Status          BIT NOT NULL CONSTRAINT DF_ExerciseMaster_Status DEFAULT (1)
    );
END
GO

-- 1.6 StaffMasters
IF OBJECT_ID(N'dbo.StaffMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffMasters (
        StaffId             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StaffMasters PRIMARY KEY,
        StaffCode           NVARCHAR(20) NULL,
        StaffName           NVARCHAR(200) NOT NULL,
        Gender              NVARCHAR(20) NOT NULL,
        MobileNo            NVARCHAR(20) NOT NULL,
        Email               NVARCHAR(100) NOT NULL,
        Designation         NVARCHAR(100) NOT NULL,
        Specializations     NVARCHAR(500) NULL,
        ExperienceYears     INT NULL,
        Salary              DECIMAL(18,2) NOT NULL CONSTRAINT DF_StaffMasters_Salary DEFAULT (0),
        JoiningDate         DATE NOT NULL,
        Address             NVARCHAR(500) NOT NULL,
        IsActive            BIT NOT NULL CONSTRAINT DF_StaffMasters_IsActive DEFAULT (1),
        RoleId              INT NOT NULL,
        BankName            NVARCHAR(150) NULL,
        BankAccountNo       NVARCHAR(50) NULL,
        IfscCode            NVARCHAR(20) NULL,
        CONSTRAINT FK_StaffMasters_RoleMaster FOREIGN KEY (RoleId) REFERENCES dbo.RoleMaster(RoleId)
    );
END
GO

-- 1.7 MemberMasters
IF OBJECT_ID(N'dbo.MemberMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberMasters (
        MemberId                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberMasters PRIMARY KEY,
        MemberCode              NVARCHAR(50) NOT NULL,
        MemberName              NVARCHAR(200) NOT NULL,
        MobileNo                NVARCHAR(20) NOT NULL,
        AlternateMobile         NVARCHAR(20) NULL,
        Email                   NVARCHAR(100) NULL,
        Gender                  NVARCHAR(20) NULL,
        DateOfBirth             DATE NULL,
        BloodGroup              NVARCHAR(10) NULL,
        Address                 NVARCHAR(500) NULL,
        City                    NVARCHAR(100) NULL,
        State                   NVARCHAR(100) NULL,
        Pincode                 NVARCHAR(20) NULL,
        EmergencyContact        NVARCHAR(20) NULL,
        EmergencyContactName    NVARCHAR(200) NULL,
        TrainerId               INT NULL,
        PlanId                  INT NULL,
        JoinDate                DATE NOT NULL,
        PlanStartDate           DATE NOT NULL,
        PlanEndDate             DATE NOT NULL,
        Height                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberMasters_Height DEFAULT (0),
        Weight                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberMasters_Weight DEFAULT (0),
        Status                  NVARCHAR(50) NOT NULL CONSTRAINT DF_MemberMasters_Status DEFAULT (N'Active'),
        MedicalNotes            NVARCHAR(MAX) NULL,
        Remarks                 NVARCHAR(MAX) NULL,
        CreatedDate             DATETIME NOT NULL CONSTRAINT DF_MemberMasters_CreatedDate DEFAULT (GETDATE()),
        ModifiedDate            DATETIME NULL,
        CONSTRAINT FK_MemberMasters_Trainer FOREIGN KEY (TrainerId) REFERENCES dbo.StaffMasters(StaffId),
        CONSTRAINT FK_MemberMasters_Plan FOREIGN KEY (PlanId) REFERENCES dbo.MembershipPlanMasters(PlanId)
    );
END
GO

-- =============================================================================
-- SECTION 2: MASTERS & TRANSACTION TABLES (Schema_Updates + fixes)
-- =============================================================================

-- 2.1 ExpenseHeadMasters
IF OBJECT_ID(N'dbo.ExpenseHeadMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseHeadMasters (
        ExpenseHeadId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExpenseHeadMasters PRIMARY KEY,
        HeadName        NVARCHAR(100) NOT NULL,
        Description     NVARCHAR(500) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_ExpenseHeadMasters_IsActive DEFAULT (1)
    );
END
GO

-- 2.2 LeadSourceMasters
IF OBJECT_ID(N'dbo.LeadSourceMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeadSourceMasters (
        LeadSourceId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeadSourceMasters PRIMARY KEY,
        SourceCode      NVARCHAR(20) NOT NULL,
        SourceName      NVARCHAR(100) NOT NULL,
        Description     NVARCHAR(500) NULL,
        DisplayOrder    INT NOT NULL CONSTRAINT DF_LeadSourceMasters_DisplayOrder DEFAULT (0),
        IsActive        BIT NOT NULL CONSTRAINT DF_LeadSourceMasters_IsActive DEFAULT (1),
        CreatedDate     DATETIME NOT NULL CONSTRAINT DF_LeadSourceMasters_CreatedDate DEFAULT (GETDATE()),
        ModifiedDate    DATETIME NULL
    );
END
GO

-- 2.3 VendorMasters
IF OBJECT_ID(N'dbo.VendorMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VendorMasters (
        VendorId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VendorMasters PRIMARY KEY,
        VendorName      NVARCHAR(200) NOT NULL,
        ContactPerson   NVARCHAR(100) NULL,
        MobileNo        NVARCHAR(20) NULL,
        Email           NVARCHAR(100) NULL,
        Address         NVARCHAR(500) NULL,
        GSTNo           NVARCHAR(50) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_VendorMasters_IsActive DEFAULT (1)
    );
END
GO

-- 2.4 DietMasters
IF OBJECT_ID(N'dbo.DietMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietMasters (
        DietId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DietMasters PRIMARY KEY,
        DietName        NVARCHAR(200) NOT NULL,
        Category        NVARCHAR(50) NOT NULL,
        MealType        NVARCHAR(50) NOT NULL,
        Calories        DECIMAL(18,2) NULL,
        Protein         DECIMAL(18,2) NULL,
        Carbs           DECIMAL(18,2) NULL,
        Fat             DECIMAL(18,2) NULL,
        Description     NVARCHAR(MAX) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_DietMasters_IsActive DEFAULT (1)
    );
END
GO

-- 2.5 EquipmentMasters
IF OBJECT_ID(N'dbo.EquipmentMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EquipmentMasters (
        EquipmentId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EquipmentMasters PRIMARY KEY,
        EquipmentName   NVARCHAR(200) NOT NULL,
        Category        NVARCHAR(100) NULL,
        PurchaseDate    DATE NULL,
        PurchasePrice   DECIMAL(18,2) NULL,
        Quantity        INT NULL,
        ConditionStatus NVARCHAR(50) NULL,
        Location        NVARCHAR(100) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_EquipmentMasters_IsActive DEFAULT (1)
    );
END
GO

-- 2.6 UserMasters
IF OBJECT_ID(N'dbo.UserMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserMasters (
        UserId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserMasters PRIMARY KEY,
        FullName        NVARCHAR(200) NOT NULL,
        UserName        NVARCHAR(100) NOT NULL,
        Email           NVARCHAR(200) NULL,
        PasswordHash    NVARCHAR(500) NOT NULL,
        RoleId          INT NOT NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_UserMasters_IsActive DEFAULT (1),
        LastLogin       DATETIME NULL,
        ProfilePhoto    NVARCHAR(500) NULL,
        CONSTRAINT UX_UserMasters_UserName UNIQUE (UserName),
        CONSTRAINT FK_UserMasters_RoleMaster FOREIGN KEY (RoleId) REFERENCES dbo.RoleMaster(RoleId)
    );
END
GO

-- 2.7 ProductMasters
IF OBJECT_ID(N'dbo.ProductMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductMasters (
        ProductId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductMasters PRIMARY KEY,
        ProductName     NVARCHAR(200) NOT NULL,
        Category        NVARCHAR(100) NULL,
        UnitPrice       DECIMAL(18,2) NOT NULL,
        CurrentStock    INT NOT NULL CONSTRAINT DF_ProductMasters_CurrentStock DEFAULT (0),
        ReorderLevel    INT NOT NULL CONSTRAINT DF_ProductMasters_ReorderLevel DEFAULT (0),
        VendorId        INT NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_ProductMasters_IsActive DEFAULT (1),
        CONSTRAINT FK_ProductMasters_Vendor FOREIGN KEY (VendorId) REFERENCES dbo.VendorMasters(VendorId)
    );
END
GO

-- 2.8 ClassMasters
IF OBJECT_ID(N'dbo.ClassMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassMasters (
        ClassId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClassMasters PRIMARY KEY,
        ClassName       NVARCHAR(200) NOT NULL,
        TrainerId       INT NULL,
        Schedule        NVARCHAR(100) NULL,
        StartTime       TIME NULL,
        EndTime         TIME NULL,
        MaxCapacity     INT NULL,
        Amount          DECIMAL(18,2) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_ClassMasters_IsActive DEFAULT (1),
        CONSTRAINT FK_ClassMasters_Trainer FOREIGN KEY (TrainerId) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- 2.9 Attendances
IF OBJECT_ID(N'dbo.Attendances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attendances (
        AttendanceId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Attendances PRIMARY KEY,
        MemberId        INT NOT NULL,
        AttendanceDate  DATE NOT NULL,
        CheckInTime     TIME NULL,
        CheckOutTime    TIME NULL,
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_Attendances_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId)
    );
END
GO

-- 2.10 MembershipTransactions
IF OBJECT_ID(N'dbo.MembershipTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MembershipTransactions (
        TransactionId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MembershipTransactions PRIMARY KEY,
        MemberId        INT NOT NULL,
        PlanId          INT NOT NULL,
        StartDate       DATE NULL,
        EndDate         DATE NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        PaymentMode     NVARCHAR(50) NULL,
        PaidDate        DATE NOT NULL,
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_MembershipTransactions_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId),
        CONSTRAINT FK_MembershipTransactions_Plan FOREIGN KEY (PlanId) REFERENCES dbo.MembershipPlanMasters(PlanId)
    );
END
GO

-- 2.11 Leads (full CRM columns)
IF OBJECT_ID(N'dbo.Leads', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Leads (
        LeadId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Leads PRIMARY KEY,
        LeadCode        NVARCHAR(20) NULL,
        LeadName        NVARCHAR(200) NOT NULL,
        MobileNo        NVARCHAR(20) NOT NULL,
        AlternateMobile NVARCHAR(20) NULL,
        Email           NVARCHAR(100) NULL,
        Gender          NVARCHAR(20) NULL,
        Address         NVARCHAR(500) NULL,
        InterestedIn    NVARCHAR(100) NULL,
        LeadSourceId    INT NULL,
        AssignedTo      INT NULL,
        Status          NVARCHAR(50) NOT NULL CONSTRAINT DF_Leads_Status DEFAULT (N'New'),
        Budget          DECIMAL(18,2) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        FollowUpDate    DATE NULL,
        IsConverted     BIT NOT NULL CONSTRAINT DF_Leads_IsConverted DEFAULT (0),
        IsActive        BIT NOT NULL CONSTRAINT DF_Leads_IsActive DEFAULT (1),
        CreatedDate     DATETIME NOT NULL CONSTRAINT DF_Leads_CreatedDate DEFAULT (GETDATE()),
        ModifiedDate    DATETIME NULL,
        CONSTRAINT FK_Leads_LeadSource FOREIGN KEY (LeadSourceId) REFERENCES dbo.LeadSourceMasters(LeadSourceId),
        CONSTRAINT FK_Leads_AssignedTo FOREIGN KEY (AssignedTo) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- 2.12 TrainerAssignments
IF OBJECT_ID(N'dbo.TrainerAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerAssignments (
        AssignmentId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrainerAssignments PRIMARY KEY,
        MemberId        INT NOT NULL,
        TrainerId       INT NOT NULL,
        StartDate       DATE NULL,
        EndDate         DATE NULL,
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_TrainerAssignments_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId),
        CONSTRAINT FK_TrainerAssignments_Trainer FOREIGN KEY (TrainerId) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- 2.13 WorkoutPlans
IF OBJECT_ID(N'dbo.WorkoutPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutPlans (
        PlanId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkoutPlans PRIMARY KEY,
        MemberId        INT NOT NULL,
        TrainerId       INT NULL,
        PlanName        NVARCHAR(200) NOT NULL,
        StartDate       DATE NULL,
        EndDate         DATE NULL,
        Goals           NVARCHAR(MAX) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        CONSTRAINT FK_WorkoutPlans_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId),
        CONSTRAINT FK_WorkoutPlans_Trainer FOREIGN KEY (TrainerId) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- 2.14 WorkoutPlanDetails
IF OBJECT_ID(N'dbo.WorkoutPlanDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutPlanDetails (
        DetailId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkoutPlanDetails PRIMARY KEY,
        PlanId          INT NOT NULL,
        ExerciseId      INT NOT NULL,
        Sets            INT NULL,
        Reps            INT NULL,
        Duration        NVARCHAR(50) NULL,
        DayOfWeek       NVARCHAR(50) NULL,
        CONSTRAINT FK_WorkoutPlanDetails_Plan FOREIGN KEY (PlanId) REFERENCES dbo.WorkoutPlans(PlanId) ON DELETE CASCADE,
        CONSTRAINT FK_WorkoutPlanDetails_Exercise FOREIGN KEY (ExerciseId) REFERENCES dbo.ExerciseMaster(ExerciseId)
    );
END
GO

-- 2.15 DietPlans
IF OBJECT_ID(N'dbo.DietPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietPlans (
        DietPlanId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DietPlans PRIMARY KEY,
        MemberId        INT NOT NULL,
        PlanName        NVARCHAR(200) NOT NULL,
        StartDate       DATE NULL,
        EndDate         DATE NULL,
        CalorieTarget   DECIMAL(18,2) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        CONSTRAINT FK_DietPlans_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId)
    );
END
GO

-- 2.16 DietPlanDetails
IF OBJECT_ID(N'dbo.DietPlanDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietPlanDetails (
        DetailId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DietPlanDetails PRIMARY KEY,
        DietPlanId      INT NOT NULL,
        DietId          INT NOT NULL,
        MealTime        NVARCHAR(50) NULL,
        Quantity        NVARCHAR(100) NULL,
        CONSTRAINT FK_DietPlanDetails_Plan FOREIGN KEY (DietPlanId) REFERENCES dbo.DietPlans(DietPlanId) ON DELETE CASCADE,
        CONSTRAINT FK_DietPlanDetails_Diet FOREIGN KEY (DietId) REFERENCES dbo.DietMasters(DietId)
    );
END
GO

-- 2.17 PTSessions
IF OBJECT_ID(N'dbo.PTSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PTSessions (
        SessionId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PTSessions PRIMARY KEY,
        MemberId        INT NOT NULL,
        TrainerId       INT NOT NULL,
        SessionDate     DATE NOT NULL,
        StartTime       TIME NULL,
        EndTime         TIME NULL,
        Status          NVARCHAR(50) NOT NULL CONSTRAINT DF_PTSessions_Status DEFAULT (N'Scheduled'),
        Remarks         NVARCHAR(MAX) NULL,
        CONSTRAINT FK_PTSessions_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId),
        CONSTRAINT FK_PTSessions_Trainer FOREIGN KEY (TrainerId) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- 2.18 ClassBookings
IF OBJECT_ID(N'dbo.ClassBookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassBookings (
        BookingId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClassBookings PRIMARY KEY,
        MemberId        INT NOT NULL,
        ClassId         INT NOT NULL,
        BookingDate     DATE NOT NULL,
        Status          NVARCHAR(50) NOT NULL CONSTRAINT DF_ClassBookings_Status DEFAULT (N'Confirmed'),
        Amount          DECIMAL(18,2) NULL,
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_ClassBookings_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId),
        CONSTRAINT FK_ClassBookings_Class FOREIGN KEY (ClassId) REFERENCES dbo.ClassMasters(ClassId)
    );
END
GO

-- 2.19 StockPurchases
IF OBJECT_ID(N'dbo.StockPurchases', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockPurchases (
        PurchaseId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockPurchases PRIMARY KEY,
        ProductId       INT NOT NULL,
        VendorId        INT NOT NULL,
        Quantity        INT NOT NULL,
        UnitPrice       DECIMAL(18,2) NOT NULL,
        TotalAmount     DECIMAL(18,2) NOT NULL,
        PurchaseDate    DATE NOT NULL,
        InvoiceNo       NVARCHAR(100) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        CONSTRAINT FK_StockPurchases_Product FOREIGN KEY (ProductId) REFERENCES dbo.ProductMasters(ProductId),
        CONSTRAINT FK_StockPurchases_Vendor FOREIGN KEY (VendorId) REFERENCES dbo.VendorMasters(VendorId)
    );
END
GO

-- 2.20 Expenses
IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses (
        ExpenseId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY,
        ExpenseHeadId   INT NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        ExpenseDate     DATE NOT NULL,
        Description     NVARCHAR(MAX) NULL,
        PaymentMode     NVARCHAR(50) NULL,
        PaidTo          NVARCHAR(200) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Expenses_Head FOREIGN KEY (ExpenseHeadId) REFERENCES dbo.ExpenseHeadMasters(ExpenseHeadId)
    );
END
GO

-- 2.21 StaffAttendances
IF OBJECT_ID(N'dbo.StaffAttendances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffAttendances (
        AttendanceId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StaffAttendances PRIMARY KEY,
        StaffId         INT NOT NULL,
        AttendanceDate  DATE NOT NULL,
        CheckInTime     TIME NULL,
        CheckOutTime    TIME NULL,
        Status          NVARCHAR(50) NULL,
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_StaffAttendances_Staff FOREIGN KEY (StaffId) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- 2.22 SalaryProcessings
IF OBJECT_ID(N'dbo.SalaryProcessings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalaryProcessings (
        SalaryId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalaryProcessings PRIMARY KEY,
        StaffId         INT NOT NULL,
        Month           INT NOT NULL,
        Year            INT NOT NULL,
        BasicSalary     DECIMAL(18,2) NOT NULL,
        Deductions      DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalaryProcessings_Deductions DEFAULT (0),
        NetSalary       DECIMAL(18,2) NOT NULL,
        PaidDate        DATE NULL,
        PaymentMode     NVARCHAR(50) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        PresentDays     INT NULL,
        AbsentDays      INT NULL,
        LeaveDays       INT NULL,
        HalfDays        INT NULL,
        SalaryRuleId    INT NULL,
        CONSTRAINT FK_SalaryProcessings_Staff FOREIGN KEY (StaffId) REFERENCES dbo.StaffMasters(StaffId)
    );
END
GO

-- =============================================================================
-- SECTION 3: ENTRIES (StockIssues, EquipmentMaintenances)
-- =============================================================================

IF OBJECT_ID(N'dbo.StockIssues', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockIssues (
        IssueId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockIssues PRIMARY KEY,
        ProductId       INT NOT NULL,
        MemberId        INT NULL,
        Quantity        INT NOT NULL,
        IssueDate       DATE NOT NULL,
        IssuedTo        NVARCHAR(200) NOT NULL,
        Amount          DECIMAL(18,2) NULL,
        PaymentMode     NVARCHAR(50) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        CONSTRAINT FK_StockIssues_Product FOREIGN KEY (ProductId) REFERENCES dbo.ProductMasters(ProductId),
        CONSTRAINT FK_StockIssues_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId)
    );
END
GO

IF OBJECT_ID(N'dbo.EquipmentMaintenances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EquipmentMaintenances (
        MaintenanceId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EquipmentMaintenances PRIMARY KEY,
        EquipmentId       INT NOT NULL,
        MaintenanceDate   DATE NOT NULL,
        MaintenanceType   NVARCHAR(100) NOT NULL,
        Cost              DECIMAL(18,2) NOT NULL,
        PaymentMode       NVARCHAR(50) NULL,
        VendorName        NVARCHAR(200) NULL,
        NextDueDate       DATE NULL,
        Status            NVARCHAR(50) NOT NULL CONSTRAINT DF_EquipmentMaintenances_Status DEFAULT (N'Completed'),
        Remarks           NVARCHAR(MAX) NULL,
        CONSTRAINT FK_EquipmentMaintenances_Equipment FOREIGN KEY (EquipmentId) REFERENCES dbo.EquipmentMasters(EquipmentId)
    );
END
GO

-- =============================================================================
-- SECTION 4: PAYMENTS
-- =============================================================================

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        MemberId        INT NOT NULL,
        PaymentDate     DATE NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        PaymentMode     NVARCHAR(50) NULL,
        ReferenceNo     NVARCHAR(100) NULL,
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_Payments_Member FOREIGN KEY (MemberId) REFERENCES dbo.MemberMasters(MemberId)
    );
END
GO

-- =============================================================================
-- SECTION 5: PAYMENT GATEWAY
-- =============================================================================

IF OBJECT_ID(N'dbo.PaymentGateways', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentGateways (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentGateways PRIMARY KEY,
        GatewayName         NVARCHAR(50) NOT NULL,
        DisplayName         NVARCHAR(100) NOT NULL,
        MerchantId          NVARCHAR(100) NULL,
        MerchantKey         NVARCHAR(500) NULL,
        MID                 NVARCHAR(100) NULL,
        ChannelId           NVARCHAR(50) NULL,
        Website             NVARCHAR(50) NULL,
        IndustryType        NVARCHAR(50) NULL,
        CallbackUrl         NVARCHAR(500) NULL,
        Environment         NVARCHAR(20) NOT NULL CONSTRAINT DF_PaymentGateways_Environment DEFAULT (N'Sandbox'),
        SandboxBaseUrl      NVARCHAR(500) NULL,
        ProductionBaseUrl   NVARCHAR(500) NULL,
        IsDefault           BIT NOT NULL CONSTRAINT DF_PaymentGateways_IsDefault DEFAULT (0),
        IsActive            BIT NOT NULL CONSTRAINT DF_PaymentGateways_IsActive DEFAULT (1),
        IsValidated         BIT NOT NULL CONSTRAINT DF_PaymentGateways_IsValidated DEFAULT (0),
        ValidationMessage   NVARCHAR(1000) NULL,
        LastValidatedOn     DATETIME2 NULL,
        CreatedBy           INT NULL,
        CreatedDate         DATETIME2 NOT NULL CONSTRAINT DF_PaymentGateways_CreatedDate DEFAULT (GETUTCDATE()),
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME2 NULL
    );
    CREATE INDEX IX_PaymentGateways_GatewayName ON dbo.PaymentGateways(GatewayName);
    CREATE INDEX IX_PaymentGateways_IsDefault ON dbo.PaymentGateways(IsDefault);
    CREATE INDEX IX_PaymentGateways_IsActive ON dbo.PaymentGateways(IsActive);
END
GO

IF OBJECT_ID(N'dbo.PaymentTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentTransactions (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentTransactions PRIMARY KEY,
        MemberId            INT NULL,
        OrderId             NVARCHAR(100) NOT NULL,
        TransactionId       NVARCHAR(100) NULL,
        Gateway             NVARCHAR(50) NOT NULL,
        Amount              DECIMAL(18,2) NOT NULL,
        Currency            NVARCHAR(10) NOT NULL CONSTRAINT DF_PaymentTransactions_Currency DEFAULT (N'INR'),
        PaymentFor          NVARCHAR(100) NULL,
        Status              NVARCHAR(50) NOT NULL CONSTRAINT DF_PaymentTransactions_Status DEFAULT (N'Pending'),
        ResponseCode        NVARCHAR(20) NULL,
        ResponseMessage     NVARCHAR(500) NULL,
        GatewayResponse     NVARCHAR(MAX) NULL,
        PaidOn              DATETIME2 NULL,
        CreatedDate         DATETIME2 NOT NULL CONSTRAINT DF_PaymentTransactions_CreatedDate DEFAULT (GETUTCDATE())
    );
    CREATE UNIQUE INDEX IX_PaymentTransactions_OrderId ON dbo.PaymentTransactions(OrderId);
    CREATE INDEX IX_PaymentTransactions_TransactionId ON dbo.PaymentTransactions(TransactionId);
    CREATE INDEX IX_PaymentTransactions_MemberId ON dbo.PaymentTransactions(MemberId);
    CREATE INDEX IX_PaymentTransactions_Status ON dbo.PaymentTransactions(Status);
END
GO

-- =============================================================================
-- SECTION 6: SALARY RULES
-- =============================================================================

IF OBJECT_ID(N'dbo.SalaryRuleMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalaryRuleMasters (
        RuleId                      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalaryRuleMasters PRIMARY KEY,
        RuleName                    NVARCHAR(150) NOT NULL,
        WorkingDaysPerMonth         INT NOT NULL CONSTRAINT DF_SalaryRule_WorkingDays DEFAULT (26),
        AbsentDeductionPerDay       BIT NOT NULL CONSTRAINT DF_SalaryRule_AbsentDed DEFAULT (1),
        HalfDayDeductionFactor      DECIMAL(4,2) NOT NULL CONSTRAINT DF_SalaryRule_HalfDay DEFAULT (0.50),
        LeaveIsPaid                 BIT NOT NULL CONSTRAINT DF_SalaryRule_LeavePaid DEFAULT (1),
        LateGraceMinutes            INT NOT NULL CONSTRAINT DF_SalaryRule_LateGrace DEFAULT (15),
        Description                 NVARCHAR(MAX) NULL,
        IsActive                    BIT NOT NULL CONSTRAINT DF_SalaryRule_IsActive DEFAULT (1),
        IsDefault                   BIT NOT NULL CONSTRAINT DF_SalaryRule_IsDefault DEFAULT (0),
        CreatedDate                 DATETIME NOT NULL CONSTRAINT DF_SalaryRule_CreatedDate DEFAULT (GETDATE()),
        ShiftStartTime              TIME NULL,
        ShiftEndTime                TIME NULL,
        EarlyLeaveGraceMinutes      INT NOT NULL CONSTRAINT DF_SalaryRule_EarlyLeaveGrace DEFAULT (0),
        EnableSandwichRule          BIT NOT NULL CONSTRAINT DF_SalaryRule_Sandwich DEFAULT (0),
        WeeklyOffDays               NVARCHAR(100) NULL,
        LateCountsAsHalfDay         BIT NOT NULL CONSTRAINT DF_SalaryRule_LateHalf DEFAULT (1),
        EarlyLeaveCountsAsHalfDay   BIT NOT NULL CONSTRAINT DF_SalaryRule_EarlyHalf DEFAULT (1)
    );
END
GO

-- =============================================================================
-- SECTION 7: WHATSAPP
-- =============================================================================

IF OBJECT_ID(N'dbo.WhatsAppApiSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WhatsAppApiSettings (
        Id                  INT NOT NULL CONSTRAINT PK_WhatsAppApiSettings PRIMARY KEY DEFAULT (1),
        IsEnabled           BIT NOT NULL CONSTRAINT DF_WhatsAppApiSettings_IsEnabled DEFAULT (0),
        ApiProvider         NVARCHAR(50) NOT NULL CONSTRAINT DF_WA_ApiProvider DEFAULT (N'SmartPing'),
        ApiBaseUrl          NVARCHAR(500) NULL,
        PhoneNumberId       NVARCHAR(100) NULL,
        BusinessPhone       NVARCHAR(20) NULL,
        WabaId              NVARCHAR(100) NULL,
        AppId               NVARCHAR(100) NULL,
        AccessToken         NVARCHAR(1000) NULL,
        VerifyToken         NVARCHAR(200) NULL,
        GraphApiVersion     NVARCHAR(20) NOT NULL CONSTRAINT DF_WhatsAppApiSettings_GraphApiVersion DEFAULT (N'v21.0'),
        WelcomeMessage      NVARCHAR(500) NULL,
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME NULL,
        CONSTRAINT CK_WhatsAppApiSettings_SingleRow CHECK (Id = 1)
    );

    INSERT INTO dbo.WhatsAppApiSettings (Id, IsEnabled, ApiProvider, VerifyToken, GraphApiVersion, WelcomeMessage)
    VALUES (1, 0, N'SmartPing', N'cloudmex-verify-token', N'v21.0', N'Welcome to CloudMex Gym!');
END
GO

IF OBJECT_ID(N'dbo.WhatsAppBotSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WhatsAppBotSessions (
        SessionId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WhatsAppBotSessions PRIMARY KEY,
        LeadId              INT NOT NULL,
        PhoneNumber         NVARCHAR(20) NOT NULL,
        CurrentStep         NVARCHAR(50) NOT NULL CONSTRAINT DF_WhatsAppBotSessions_CurrentStep DEFAULT (N'SelectTrainer'),
        SelectedTrainerId   INT NULL,
        SelectedClassId     INT NULL,
        SelectedPlanId      INT NULL,
        PaymentToken        NVARCHAR(100) NULL,
        IsCompleted         BIT NOT NULL CONSTRAINT DF_WhatsAppBotSessions_IsCompleted DEFAULT (0),
        CreatedDate         DATETIME NOT NULL CONSTRAINT DF_WhatsAppBotSessions_CreatedDate DEFAULT (GETDATE()),
        UpdatedDate         DATETIME NULL,
        CONSTRAINT FK_WhatsAppBotSessions_Leads FOREIGN KEY (LeadId) REFERENCES dbo.Leads(LeadId)
    );
    CREATE INDEX IX_WhatsAppBotSessions_Phone ON dbo.WhatsAppBotSessions(PhoneNumber);
    CREATE INDEX IX_WhatsAppBotSessions_Lead ON dbo.WhatsAppBotSessions(LeadId);
    CREATE INDEX IX_WhatsAppBotSessions_PaymentToken ON dbo.WhatsAppBotSessions(PaymentToken);
END
GO

-- =============================================================================
-- SECTION 8: COLUMN UPDATES (existing DB upgrade — safe to re-run)
-- =============================================================================

-- LeadSourceMasters extra columns (old Schema_Updates.sql)
IF OBJECT_ID(N'dbo.LeadSourceMasters', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('LeadSourceMasters', 'SourceCode') IS NULL
        ALTER TABLE dbo.LeadSourceMasters ADD SourceCode NVARCHAR(20) NOT NULL CONSTRAINT DF_LeadSourceMasters_SourceCode DEFAULT (N'LS');
    IF COL_LENGTH('LeadSourceMasters', 'DisplayOrder') IS NULL
        ALTER TABLE dbo.LeadSourceMasters ADD DisplayOrder INT NOT NULL CONSTRAINT DF_LeadSourceMasters_DisplayOrder2 DEFAULT (0);
    IF COL_LENGTH('LeadSourceMasters', 'CreatedDate') IS NULL
        ALTER TABLE dbo.LeadSourceMasters ADD CreatedDate DATETIME NOT NULL CONSTRAINT DF_LeadSourceMasters_CreatedDate2 DEFAULT (GETDATE());
    IF COL_LENGTH('LeadSourceMasters', 'ModifiedDate') IS NULL
        ALTER TABLE dbo.LeadSourceMasters ADD ModifiedDate DATETIME NULL;
END
GO

-- UserMasters extra columns
IF OBJECT_ID(N'dbo.UserMasters', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('UserMasters', 'Email') IS NULL
        ALTER TABLE dbo.UserMasters ADD Email NVARCHAR(200) NULL;
    IF COL_LENGTH('UserMasters', 'LastLogin') IS NULL
        ALTER TABLE dbo.UserMasters ADD LastLogin DATETIME NULL;
    IF COL_LENGTH('UserMasters', 'ProfilePhoto') IS NULL
        ALTER TABLE dbo.UserMasters ADD ProfilePhoto NVARCHAR(500) NULL;
END
GO

-- StaffMasters: trainer expertise + bank + staff code
IF OBJECT_ID(N'dbo.StaffMasters', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('StaffMasters', 'Specializations') IS NULL
        ALTER TABLE dbo.StaffMasters ADD Specializations NVARCHAR(500) NULL;
    IF COL_LENGTH('StaffMasters', 'ExperienceYears') IS NULL
        ALTER TABLE dbo.StaffMasters ADD ExperienceYears INT NULL;
    IF COL_LENGTH('StaffMasters', 'StaffCode') IS NULL
        ALTER TABLE dbo.StaffMasters ADD StaffCode NVARCHAR(20) NULL;
    IF COL_LENGTH('StaffMasters', 'BankName') IS NULL
        ALTER TABLE dbo.StaffMasters ADD BankName NVARCHAR(150) NULL;
    IF COL_LENGTH('StaffMasters', 'BankAccountNo') IS NULL
        ALTER TABLE dbo.StaffMasters ADD BankAccountNo NVARCHAR(50) NULL;
    IF COL_LENGTH('StaffMasters', 'IfscCode') IS NULL
        ALTER TABLE dbo.StaffMasters ADD IfscCode NVARCHAR(20) NULL;
END
GO

UPDATE dbo.StaffMasters
SET StaffCode = N'EMP' + RIGHT(N'000' + CAST(StaffId AS NVARCHAR(10)), 3)
WHERE StaffCode IS NULL OR LTRIM(RTRIM(StaffCode)) = N'';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_StaffMasters_StaffCode' AND object_id = OBJECT_ID(N'dbo.StaffMasters'))
BEGIN
    CREATE UNIQUE INDEX UX_StaffMasters_StaffCode ON dbo.StaffMasters(StaffCode)
    WHERE StaffCode IS NOT NULL AND LTRIM(RTRIM(StaffCode)) <> N'';
END
GO

-- Leads CRM columns
IF OBJECT_ID(N'dbo.Leads', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Leads', 'LeadCode') IS NULL
        ALTER TABLE dbo.Leads ADD LeadCode NVARCHAR(20) NULL;
    IF COL_LENGTH('Leads', 'AlternateMobile') IS NULL
        ALTER TABLE dbo.Leads ADD AlternateMobile NVARCHAR(20) NULL;
    IF COL_LENGTH('Leads', 'Gender') IS NULL
        ALTER TABLE dbo.Leads ADD Gender NVARCHAR(20) NULL;
    IF COL_LENGTH('Leads', 'Address') IS NULL
        ALTER TABLE dbo.Leads ADD Address NVARCHAR(500) NULL;
    IF COL_LENGTH('Leads', 'Budget') IS NULL
        ALTER TABLE dbo.Leads ADD Budget DECIMAL(18,2) NULL;
    IF COL_LENGTH('Leads', 'IsConverted') IS NULL
        ALTER TABLE dbo.Leads ADD IsConverted BIT NOT NULL CONSTRAINT DF_Leads_IsConverted2 DEFAULT (0);
    IF COL_LENGTH('Leads', 'IsActive') IS NULL
        ALTER TABLE dbo.Leads ADD IsActive BIT NOT NULL CONSTRAINT DF_Leads_IsActive2 DEFAULT (1);
    IF COL_LENGTH('Leads', 'ModifiedDate') IS NULL
        ALTER TABLE dbo.Leads ADD ModifiedDate DATETIME NULL;
END
GO

-- SalaryProcessings breakdown
IF OBJECT_ID(N'dbo.SalaryProcessings', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('SalaryProcessings', 'PresentDays') IS NULL
        ALTER TABLE dbo.SalaryProcessings ADD PresentDays INT NULL;
    IF COL_LENGTH('SalaryProcessings', 'AbsentDays') IS NULL
        ALTER TABLE dbo.SalaryProcessings ADD AbsentDays INT NULL;
    IF COL_LENGTH('SalaryProcessings', 'LeaveDays') IS NULL
        ALTER TABLE dbo.SalaryProcessings ADD LeaveDays INT NULL;
    IF COL_LENGTH('SalaryProcessings', 'HalfDays') IS NULL
        ALTER TABLE dbo.SalaryProcessings ADD HalfDays INT NULL;
    IF COL_LENGTH('SalaryProcessings', 'SalaryRuleId') IS NULL
        ALTER TABLE dbo.SalaryProcessings ADD SalaryRuleId INT NULL;
END
GO

-- SalaryRuleMasters shift/sandwich columns
IF OBJECT_ID(N'dbo.SalaryRuleMasters', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('SalaryRuleMasters', 'ShiftStartTime') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD ShiftStartTime TIME NULL;
    IF COL_LENGTH('SalaryRuleMasters', 'ShiftEndTime') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD ShiftEndTime TIME NULL;
    IF COL_LENGTH('SalaryRuleMasters', 'EarlyLeaveGraceMinutes') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD EarlyLeaveGraceMinutes INT NOT NULL CONSTRAINT DF_SalaryRule_EarlyLeaveGrace DEFAULT (0);
    IF COL_LENGTH('SalaryRuleMasters', 'EnableSandwichRule') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD EnableSandwichRule BIT NOT NULL CONSTRAINT DF_SalaryRule_Sandwich DEFAULT (0);
    IF COL_LENGTH('SalaryRuleMasters', 'WeeklyOffDays') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD WeeklyOffDays NVARCHAR(100) NULL;
    IF COL_LENGTH('SalaryRuleMasters', 'LateCountsAsHalfDay') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD LateCountsAsHalfDay BIT NOT NULL CONSTRAINT DF_SalaryRule_LateHalf DEFAULT (1);
    IF COL_LENGTH('SalaryRuleMasters', 'EarlyLeaveCountsAsHalfDay') IS NULL
        ALTER TABLE dbo.SalaryRuleMasters ADD EarlyLeaveCountsAsHalfDay BIT NOT NULL CONSTRAINT DF_SalaryRule_EarlyHalf DEFAULT (1);
END
GO

-- WhatsApp SmartPing columns
IF OBJECT_ID(N'dbo.WhatsAppApiSettings', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('WhatsAppApiSettings', 'ApiProvider') IS NULL
        ALTER TABLE dbo.WhatsAppApiSettings ADD ApiProvider NVARCHAR(50) NOT NULL CONSTRAINT DF_WA_ApiProvider2 DEFAULT (N'SmartPing');
    IF COL_LENGTH('WhatsAppApiSettings', 'ApiBaseUrl') IS NULL
        ALTER TABLE dbo.WhatsAppApiSettings ADD ApiBaseUrl NVARCHAR(500) NULL;

    UPDATE dbo.WhatsAppApiSettings
    SET ApiProvider = N'SmartPing'
    WHERE ApiProvider IS NULL OR LTRIM(RTRIM(ApiProvider)) = N'';
END
GO

-- =============================================================================
-- SECTION 9: SEED DATA — Roles
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM dbo.RoleMaster WHERE RoleId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.RoleMaster ON;
    INSERT INTO dbo.RoleMaster (RoleId, RoleName, Description, IsActive)
    VALUES (1, N'Super Admin', N'Full system access — all permissions bypass', 1);
    SET IDENTITY_INSERT dbo.RoleMaster OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RoleMaster WHERE RoleName = N'Trainer')
BEGIN
    INSERT INTO dbo.RoleMaster (RoleName, Description, IsActive)
    VALUES (N'Trainer', N'Gym trainer / coach', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RoleMaster WHERE RoleName = N'Reception')
BEGIN
    INSERT INTO dbo.RoleMaster (RoleName, Description, IsActive)
    VALUES (N'Reception', N'Front desk staff', 1);
END
GO

-- =============================================================================
-- SECTION 10: SEED DATA — Permissions (all modules)
-- =============================================================================

;WITH ModuleSeed AS (
    SELECT * FROM (VALUES
        (N'Dashboard',              N'Dashboard',                   1),
        (N'MembershipPlans',        N'Membership Plans',            10),
        (N'MemberMaster',           N'Member Master',               11),
        (N'StaffMaster',            N'Staff Master',                12),
        (N'ExerciseMaster',         N'Exercise Master',             13),
        (N'DietMaster',             N'Diet Master',                 14),
        (N'Classes',                N'Class Master',                15),
        (N'Equipment',              N'Equipment Master',            16),
        (N'Products',               N'Products',                    17),
        (N'Vendors',                N'Vendors',                     18),
        (N'ExpenseHeads',           N'Expense Heads',               19),
        (N'PaymentGateway',         N'Payment Gateway',             20),
        (N'WhatsAppApiSetup',       N'WhatsApp API Setup',          21),
        (N'LeadSources',            N'Lead Sources',                22),
        (N'UsersRoles',             N'Users & Roles',               23),
        (N'MembershipManagement',     N'Membership Management',       30),
        (N'Attendance',             N'Attendance',                  31),
        (N'Payments',               N'Payments',                    32),
        (N'Leads',                  N'Leads',                       33),
        (N'TrainerAssignment',      N'Trainer Assignment',          34),
        (N'WorkoutPlans',           N'Workout Plans',               35),
        (N'PTSessions',             N'PT Sessions',                 36),
        (N'ClassBookings',          N'Class Bookings',              37),
        (N'StockPurchase',          N'Stock Purchase',              38),
        (N'StockIssue',             N'Stock Issue',                 39),
        (N'EquipmentMaintenance',   N'Equipment Maintenance',       40),
        (N'Expenses',               N'Expenses',                    41),
        (N'StaffAttendance',        N'Staff Attendance',            42),
        (N'SalaryProcessing',       N'Salary Processing',           43),
        (N'Members',                N'Members',                     44),
        (N'Payroll',                N'Payroll',                     88),
        (N'SalaryRuleMaster',       N'Salary Rule Master',          94),
        (N'DietPlans',              N'Diet Plans',                  95),
        (N'WhatsAppBot',            N'WhatsApp Bot',                96),
        (N'ReportAttendance',       N'Report - Attendance',         200),
        (N'ReportExpiry',           N'Report - Membership Expiry',  201),
        (N'ReportCollections',      N'Report - Collections',        202),
        (N'ReportOutstanding',      N'Report - Outstanding',        203),
        (N'ReportProfitLoss',       N'Report - Profit/Loss',        204)
    ) AS V(ModuleName, DisplayName, SortOrder)
)
INSERT INTO dbo.PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
SELECT M.ModuleName, M.DisplayName, M.SortOrder, 1
FROM ModuleSeed M
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PermissionMaster P WHERE P.ModuleName = M.ModuleName
);
GO

-- Super Admin — full access to all permissions
INSERT INTO dbo.RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT 1, P.PermissionId, 1, 1, 1, 1, 1
FROM dbo.PermissionMaster P
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermission RP
    WHERE RP.RoleId = 1 AND RP.PermissionId = P.PermissionId
);
GO

-- Reports: view + export only for Super Admin (if newly added)
UPDATE dbo.RolePermission
SET CanView = 1, CanExport = 1, CanAdd = 0, CanEdit = 0, CanDelete = 0
FROM dbo.RolePermission RP
INNER JOIN dbo.PermissionMaster P ON P.PermissionId = RP.PermissionId
WHERE RP.RoleId = 1
  AND P.ModuleName IN (
      N'ReportAttendance', N'ReportExpiry', N'ReportCollections',
      N'ReportOutstanding', N'ReportProfitLoss'
  );
GO

-- Payroll permission copy from SalaryProcessing (for non-admin roles)
INSERT INTO dbo.RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT SP.RoleId, PP.PermissionId,
       SP.CanView, SP.CanAdd, SP.CanEdit, SP.CanDelete, SP.CanExport
FROM dbo.RolePermission SP
INNER JOIN dbo.PermissionMaster PM ON PM.PermissionId = SP.PermissionId AND PM.ModuleName = N'SalaryProcessing'
CROSS JOIN dbo.PermissionMaster PP
WHERE PP.ModuleName = N'Payroll'
AND SP.RoleId <> 1
AND NOT EXISTS (
    SELECT 1 FROM dbo.RolePermission RP
    WHERE RP.RoleId = SP.RoleId AND RP.PermissionId = PP.PermissionId
);
GO

-- =============================================================================
-- SECTION 11: SEED DATA — Lead Sources
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM dbo.LeadSourceMasters WHERE SourceName = N'Walk-in')
BEGIN
    INSERT INTO dbo.LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES (N'LSWI', N'Walk-in', N'Walk-in enquiry at gym', 1, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LeadSourceMasters WHERE SourceName = N'Referral')
BEGIN
    INSERT INTO dbo.LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES (N'LSRF', N'Referral', N'Member or staff referral', 2, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LeadSourceMasters WHERE SourceName = N'Social Media')
BEGIN
    INSERT INTO dbo.LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES (N'LSSM', N'Social Media', N'Instagram, Facebook, etc.', 3, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LeadSourceMasters WHERE SourceName = N'WhatsApp Bot')
BEGIN
    INSERT INTO dbo.LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES (N'LSWA', N'WhatsApp Bot', N'Leads from WhatsApp bot / public join form', 99, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LeadSourceMasters WHERE SourceName = N'Public Form')
BEGIN
    INSERT INTO dbo.LeadSourceMasters (SourceCode, SourceName, Description, DisplayOrder, IsActive)
    VALUES (N'LSPF', N'Public Form', N'Leads from public join form link', 98, 1);
END
GO

-- =============================================================================
-- SECTION 12: ADMIN USER (manual — BCrypt hash required)
-- =============================================================================
/*
-- Password hash generate karein (C#):
-- BCrypt.Net.BCrypt.HashPassword("Admin@123")

IF NOT EXISTS (SELECT 1 FROM dbo.UserMasters WHERE UserName = N'admin')
BEGIN
    INSERT INTO dbo.UserMasters (FullName, UserName, Email, PasswordHash, RoleId, IsActive)
    VALUES (
        N'Super Admin',
        N'admin',
        N'admin@cloudmexgym.com',
        N'<PASTE_BCRYPT_HASH_HERE>',
        1,
        1
    );
END
GO
*/

PRINT N'CloudMex Gym — Full_Database.sql completed successfully.';
PRINT N'Next: UserMasters mein admin user insert karein (BCrypt hash ke saath).';
GO

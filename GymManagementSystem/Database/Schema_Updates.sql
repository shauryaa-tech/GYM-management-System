-- Schema Updates for Gym Management System

-- 1. ExpenseHeadMasters
CREATE TABLE ExpenseHeadMasters (
    ExpenseHeadId INT IDENTITY(1,1) PRIMARY KEY,
    HeadName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- 2. LeadSourceMasters
CREATE TABLE LeadSourceMasters (
    LeadSourceId INT IDENTITY(1,1) PRIMARY KEY,
    SourceName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- 3. VendorMasters
CREATE TABLE VendorMasters (
    VendorId INT IDENTITY(1,1) PRIMARY KEY,
    VendorName NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(100),
    MobileNo NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    GSTNo NVARCHAR(50),
    IsActive BIT DEFAULT 1
);

-- 4. DietMasters
CREATE TABLE DietMasters (
    DietId INT IDENTITY(1,1) PRIMARY KEY,
    DietName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(50) NOT NULL, -- Veg/Non-Veg/Vegan
    MealType NVARCHAR(50) NOT NULL, -- Breakfast/Lunch/Dinner/Snack
    Calories DECIMAL(18,2),
    Protein DECIMAL(18,2),
    Carbs DECIMAL(18,2),
    Fat DECIMAL(18,2),
    Description NVARCHAR(MAX),
    IsActive BIT DEFAULT 1
);

-- 5. EquipmentMasters
CREATE TABLE EquipmentMasters (
    EquipmentId INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(100),
    PurchaseDate DATE,
    PurchasePrice DECIMAL(18,2),
    Quantity INT,
    ConditionStatus NVARCHAR(50), -- Good/Fair/Poor
    Location NVARCHAR(100),
    Remarks NVARCHAR(MAX),
    IsActive BIT DEFAULT 1
);

-- 6. UserMasters (Depends on RoleMasters)
CREATE TABLE UserMasters (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    UserName NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (RoleId) REFERENCES RoleMasters(RoleId)
);

-- 7. ProductMasters (Depends on VendorMasters)
CREATE TABLE ProductMasters (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(100), -- Supplement/Apparel/Accessory/Other
    UnitPrice DECIMAL(18,2) NOT NULL,
    CurrentStock INT DEFAULT 0,
    ReorderLevel INT DEFAULT 0,
    VendorId INT,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (VendorId) REFERENCES VendorMasters(VendorId)
);

-- 8. ClassMasters (Depends on StaffMasters)
CREATE TABLE ClassMasters (
    ClassId INT IDENTITY(1,1) PRIMARY KEY,
    ClassName NVARCHAR(200) NOT NULL,
    TrainerId INT,
    Schedule NVARCHAR(100), -- Mon/Wed/Fri
    StartTime TIME,
    EndTime TIME,
    MaxCapacity INT,
    Amount DECIMAL(18,2),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (TrainerId) REFERENCES StaffMasters(StaffId)
);

-- 9. Attendances (Depends on MemberMasters)
CREATE TABLE Attendances (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    CheckInTime TIME,
    CheckOutTime TIME,
    Remarks NVARCHAR(500),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId)
);

-- 10. MembershipTransactions (Depends on MemberMasters, MembershipPlanMasters)
CREATE TABLE MembershipTransactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    PlanId INT NOT NULL,
    StartDate DATE,
    EndDate DATE,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMode NVARCHAR(50), -- Cash/UPI/Card/Bank
    PaidDate DATE NOT NULL,
    Remarks NVARCHAR(500),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId),
    FOREIGN KEY (PlanId) REFERENCES MembershipPlanMasters(PlanId)
);

-- 11. Leads (Depends on LeadSourceMasters, StaffMasters)
CREATE TABLE Leads (
    LeadId INT IDENTITY(1,1) PRIMARY KEY,
    LeadName NVARCHAR(200) NOT NULL,
    MobileNo NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100),
    InterestedIn NVARCHAR(100),
    LeadSourceId INT,
    AssignedTo INT, -- StaffId
    Status NVARCHAR(50) DEFAULT 'New', -- New/Contacted/Interested/Converted/Lost
    Remarks NVARCHAR(MAX),
    FollowUpDate DATE,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (LeadSourceId) REFERENCES LeadSourceMasters(LeadSourceId),
    FOREIGN KEY (AssignedTo) REFERENCES StaffMasters(StaffId)
);

-- 12. TrainerAssignments (Depends on MemberMasters, StaffMasters)
CREATE TABLE TrainerAssignments (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    TrainerId INT NOT NULL,
    StartDate DATE,
    EndDate DATE,
    Remarks NVARCHAR(500),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId),
    FOREIGN KEY (TrainerId) REFERENCES StaffMasters(StaffId)
);

-- 13. WorkoutPlans (Depends on MemberMasters, StaffMasters)
CREATE TABLE WorkoutPlans (
    PlanId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    TrainerId INT,
    PlanName NVARCHAR(200) NOT NULL,
    StartDate DATE,
    EndDate DATE,
    Goals NVARCHAR(MAX),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId),
    FOREIGN KEY (TrainerId) REFERENCES StaffMasters(StaffId)
);

-- 14. WorkoutPlanDetails (Depends on WorkoutPlans, ExerciseMasters)
CREATE TABLE WorkoutPlanDetails (
    DetailId INT IDENTITY(1,1) PRIMARY KEY,
    PlanId INT NOT NULL,
    ExerciseId INT NOT NULL,
    Sets INT,
    Reps INT,
    Duration NVARCHAR(50), -- e.g., '30 mins'
    DayOfWeek NVARCHAR(50), -- Mon, Tue, etc.
    FOREIGN KEY (PlanId) REFERENCES WorkoutPlans(PlanId) ON DELETE CASCADE,
    FOREIGN KEY (ExerciseId) REFERENCES ExerciseMasters(ExerciseId)
);

-- 15. DietPlans (Depends on MemberMasters)
CREATE TABLE DietPlans (
    DietPlanId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    PlanName NVARCHAR(200) NOT NULL,
    StartDate DATE,
    EndDate DATE,
    CalorieTarget DECIMAL(18,2),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId)
);

-- 16. DietPlanDetails (Depends on DietPlans, DietMasters)
CREATE TABLE DietPlanDetails (
    DetailId INT IDENTITY(1,1) PRIMARY KEY,
    DietPlanId INT NOT NULL,
    DietId INT NOT NULL,
    MealTime NVARCHAR(50), -- Morning, Lunch, etc.
    Quantity NVARCHAR(100),
    FOREIGN KEY (DietPlanId) REFERENCES DietPlans(DietPlanId) ON DELETE CASCADE,
    FOREIGN KEY (DietId) REFERENCES DietMasters(DietId)
);

-- 17. PTSessions (Depends on MemberMasters, StaffMasters)
CREATE TABLE PTSessions (
    SessionId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    TrainerId INT NOT NULL,
    SessionDate DATE NOT NULL,
    StartTime TIME,
    EndTime TIME,
    Status NVARCHAR(50) DEFAULT 'Scheduled', -- Scheduled/Completed/Cancelled
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId),
    FOREIGN KEY (TrainerId) REFERENCES StaffMasters(StaffId)
);

-- 18. ClassBookings (Depends on MemberMasters, ClassMasters)
CREATE TABLE ClassBookings (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    ClassId INT NOT NULL,
    BookingDate DATE NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Confirmed', -- Confirmed/Cancelled
    Amount DECIMAL(18,2),
    Remarks NVARCHAR(500),
    FOREIGN KEY (MemberId) REFERENCES MemberMasters(MemberId),
    FOREIGN KEY (ClassId) REFERENCES ClassMasters(ClassId)
);

-- 19. StockPurchases (Depends on ProductMasters, VendorMasters)
CREATE TABLE StockPurchases (
    PurchaseId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    VendorId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PurchaseDate DATE NOT NULL,
    InvoiceNo NVARCHAR(100),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (ProductId) REFERENCES ProductMasters(ProductId),
    FOREIGN KEY (VendorId) REFERENCES VendorMasters(VendorId)
);

-- 20. Expenses (Depends on ExpenseHeadMasters)
CREATE TABLE Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    ExpenseHeadId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ExpenseDate DATE NOT NULL,
    Description NVARCHAR(MAX),
    PaymentMode NVARCHAR(50),
    PaidTo NVARCHAR(200),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (ExpenseHeadId) REFERENCES ExpenseHeadMasters(ExpenseHeadId)
);

-- 21. StaffAttendances (Depends on StaffMasters)
CREATE TABLE StaffAttendances (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    CheckInTime TIME,
    CheckOutTime TIME,
    Status NVARCHAR(50), -- Present/Absent/Leave/HalfDay
    Remarks NVARCHAR(500),
    FOREIGN KEY (StaffId) REFERENCES StaffMasters(StaffId)
);

-- 22. SalaryProcessings (Depends on StaffMasters)
CREATE TABLE SalaryProcessings (
    SalaryId INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL,
    Month INT NOT NULL,
    Year INT NOT NULL,
    BasicSalary DECIMAL(18,2) NOT NULL,
    Deductions DECIMAL(18,2) DEFAULT 0,
    NetSalary DECIMAL(18,2) NOT NULL,
    PaidDate DATE,
    PaymentMode NVARCHAR(50),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (StaffId) REFERENCES StaffMasters(StaffId)
);

-- Salary Rule Master (admin adds rules from UI — no default data inserted)

IF OBJECT_ID('SalaryRuleMasters', 'U') IS NULL
BEGIN
    CREATE TABLE SalaryRuleMasters (
        RuleId INT IDENTITY(1,1) PRIMARY KEY,
        RuleName NVARCHAR(150) NOT NULL,
        WorkingDaysPerMonth INT NOT NULL DEFAULT 26,
        AbsentDeductionPerDay BIT NOT NULL DEFAULT 1,
        HalfDayDeductionFactor DECIMAL(4,2) NOT NULL DEFAULT 0.50,
        LeaveIsPaid BIT NOT NULL DEFAULT 1,
        LateGraceMinutes INT NOT NULL DEFAULT 15,
        Description NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDefault BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'SalaryRuleMaster')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('SalaryRuleMaster', 'Salary Rule Master', 94, 1);
END
GO

INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT 1, P.PermissionId, 1, 1, 1, 1, 0
FROM PermissionMaster P
WHERE P.ModuleName = 'SalaryRuleMaster'
AND NOT EXISTS (
    SELECT 1 FROM RolePermission RP WHERE RP.RoleId = 1 AND RP.PermissionId = P.PermissionId
);
GO

-- Salary breakdown columns (if not already added)
IF COL_LENGTH('SalaryProcessings', 'PresentDays') IS NULL
    ALTER TABLE SalaryProcessings ADD PresentDays INT NULL;
IF COL_LENGTH('SalaryProcessings', 'AbsentDays') IS NULL
    ALTER TABLE SalaryProcessings ADD AbsentDays INT NULL;
IF COL_LENGTH('SalaryProcessings', 'LeaveDays') IS NULL
    ALTER TABLE SalaryProcessings ADD LeaveDays INT NULL;
IF COL_LENGTH('SalaryProcessings', 'HalfDays') IS NULL
    ALTER TABLE SalaryProcessings ADD HalfDays INT NULL;
IF COL_LENGTH('SalaryProcessings', 'SalaryRuleId') IS NULL
    ALTER TABLE SalaryProcessings ADD SalaryRuleId INT NULL;
GO

-- Shift timing & sandwich rule columns
IF COL_LENGTH('SalaryRuleMasters', 'ShiftStartTime') IS NULL
    ALTER TABLE SalaryRuleMasters ADD ShiftStartTime TIME NULL;
IF COL_LENGTH('SalaryRuleMasters', 'ShiftEndTime') IS NULL
    ALTER TABLE SalaryRuleMasters ADD ShiftEndTime TIME NULL;
IF COL_LENGTH('SalaryRuleMasters', 'EarlyLeaveGraceMinutes') IS NULL
    ALTER TABLE SalaryRuleMasters ADD EarlyLeaveGraceMinutes INT NOT NULL DEFAULT 0;
IF COL_LENGTH('SalaryRuleMasters', 'EnableSandwichRule') IS NULL
    ALTER TABLE SalaryRuleMasters ADD EnableSandwichRule BIT NOT NULL DEFAULT 0;
IF COL_LENGTH('SalaryRuleMasters', 'WeeklyOffDays') IS NULL
    ALTER TABLE SalaryRuleMasters ADD WeeklyOffDays NVARCHAR(100) NULL;
IF COL_LENGTH('SalaryRuleMasters', 'LateCountsAsHalfDay') IS NULL
    ALTER TABLE SalaryRuleMasters ADD LateCountsAsHalfDay BIT NOT NULL DEFAULT 1;
IF COL_LENGTH('SalaryRuleMasters', 'EarlyLeaveCountsAsHalfDay') IS NULL
    ALTER TABLE SalaryRuleMasters ADD EarlyLeaveCountsAsHalfDay BIT NOT NULL DEFAULT 1;
GO

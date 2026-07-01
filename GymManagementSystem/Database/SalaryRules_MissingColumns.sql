-- Run this once if Salary Rule Master Update fails with "Invalid column name"
-- Safe to run multiple times

IF COL_LENGTH('SalaryRuleMasters', 'ShiftStartTime') IS NULL
    ALTER TABLE SalaryRuleMasters ADD ShiftStartTime TIME NULL;
IF COL_LENGTH('SalaryRuleMasters', 'ShiftEndTime') IS NULL
    ALTER TABLE SalaryRuleMasters ADD ShiftEndTime TIME NULL;
IF COL_LENGTH('SalaryRuleMasters', 'EarlyLeaveGraceMinutes') IS NULL
    ALTER TABLE SalaryRuleMasters ADD EarlyLeaveGraceMinutes INT NOT NULL CONSTRAINT DF_SalaryRule_EarlyLeaveGrace DEFAULT 0;
IF COL_LENGTH('SalaryRuleMasters', 'EnableSandwichRule') IS NULL
    ALTER TABLE SalaryRuleMasters ADD EnableSandwichRule BIT NOT NULL CONSTRAINT DF_SalaryRule_Sandwich DEFAULT 0;
IF COL_LENGTH('SalaryRuleMasters', 'WeeklyOffDays') IS NULL
    ALTER TABLE SalaryRuleMasters ADD WeeklyOffDays NVARCHAR(100) NULL;
IF COL_LENGTH('SalaryRuleMasters', 'LateCountsAsHalfDay') IS NULL
    ALTER TABLE SalaryRuleMasters ADD LateCountsAsHalfDay BIT NOT NULL CONSTRAINT DF_SalaryRule_LateHalf DEFAULT 1;
IF COL_LENGTH('SalaryRuleMasters', 'EarlyLeaveCountsAsHalfDay') IS NULL
    ALTER TABLE SalaryRuleMasters ADD EarlyLeaveCountsAsHalfDay BIT NOT NULL CONSTRAINT DF_SalaryRule_EarlyHalf DEFAULT 1;
GO

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

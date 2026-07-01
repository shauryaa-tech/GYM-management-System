-- Trainer expertise for auto-assignment (run on SQL Server)
IF COL_LENGTH('StaffMasters', 'Specializations') IS NULL
    ALTER TABLE StaffMasters ADD Specializations NVARCHAR(500) NULL;

IF COL_LENGTH('StaffMasters', 'ExperienceYears') IS NULL
    ALTER TABLE StaffMasters ADD ExperienceYears INT NULL;

GO

-- Example: UPDATE StaffMasters SET Specializations = 'Weight Loss,Muscle Gain', ExperienceYears = 5 WHERE StaffId = 1;

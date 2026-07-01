-- Payroll hub module permissions
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'Payroll')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('Payroll', 'Payroll', 88, 1);
END
GO

INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT 1, P.PermissionId, 1, 1, 1, 1, 1
FROM PermissionMaster P
WHERE P.ModuleName = 'Payroll'
AND NOT EXISTS (
    SELECT 1 FROM RolePermission RP
    WHERE RP.RoleId = 1 AND RP.PermissionId = P.PermissionId
);
GO

-- Grant Payroll view to roles that already have Salary Processing
INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT SP.RoleId, PP.PermissionId,
       SP.CanView, SP.CanAdd, SP.CanEdit, SP.CanDelete, SP.CanExport
FROM RolePermission SP
INNER JOIN PermissionMaster PM ON PM.PermissionId = SP.PermissionId AND PM.ModuleName = 'SalaryProcessing'
CROSS JOIN PermissionMaster PP
WHERE PP.ModuleName = 'Payroll'
AND NOT EXISTS (
    SELECT 1 FROM RolePermission RP
    WHERE RP.RoleId = SP.RoleId AND RP.PermissionId = PP.PermissionId
);
GO

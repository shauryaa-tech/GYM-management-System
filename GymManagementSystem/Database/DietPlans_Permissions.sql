-- Diet Plans module permissions
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'DietPlans')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('DietPlans', 'Diet Plans', 95, 1);
END
GO

INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT 1, P.PermissionId, 1, 1, 1, 1, 1
FROM PermissionMaster P
WHERE P.ModuleName = 'DietPlans'
AND NOT EXISTS (
    SELECT 1 FROM RolePermission RP
    WHERE RP.RoleId = 1 AND RP.PermissionId = P.PermissionId
);

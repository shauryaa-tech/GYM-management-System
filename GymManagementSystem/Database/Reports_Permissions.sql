-- Report module permissions for role-based access
-- Run against your Gym Management database after PermissionMaster table exists.

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'ReportAttendance')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('ReportAttendance', 'Report - Attendance', 200, 1);
END

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'ReportExpiry')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('ReportExpiry', 'Report - Membership Expiry', 201, 1);
END

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'ReportCollections')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('ReportCollections', 'Report - Collections', 202, 1);
END

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'ReportOutstanding')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('ReportOutstanding', 'Report - Outstanding', 203, 1);
END

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'ReportProfitLoss')
BEGIN
    INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
    VALUES ('ReportProfitLoss', 'Report - Profit/Loss', 204, 1);
END

-- Optional: grant all report permissions to Super Admin role (RoleId = 1)
INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
SELECT 1, P.PermissionId, 1, 0, 0, 0, 1
FROM PermissionMaster P
WHERE P.ModuleName IN (
    'ReportAttendance',
    'ReportExpiry',
    'ReportCollections',
    'ReportOutstanding',
    'ReportProfitLoss'
)
AND NOT EXISTS (
    SELECT 1
    FROM RolePermission RP
    WHERE RP.RoleId = 1
      AND RP.PermissionId = P.PermissionId
);

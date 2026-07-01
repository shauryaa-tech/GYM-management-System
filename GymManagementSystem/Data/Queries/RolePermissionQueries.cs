namespace GymManagement.Data.Queries
{
    public static class RolePermissionQueries
    {
        public const string GetByRole = @"

SELECT

P.PermissionId,

P.ModuleName,

P.DisplayName,

ISNULL(RP.CanView,0) CanView,

ISNULL(RP.CanAdd,0) CanAdd,

ISNULL(RP.CanEdit,0) CanEdit,

ISNULL(RP.CanDelete,0) CanDelete,

ISNULL(RP.CanExport,0) CanExport

FROM PermissionMaster P

LEFT JOIN RolePermission RP

ON RP.PermissionId=P.PermissionId

AND RP.RoleId=@RoleId

ORDER BY P.SortOrder";



        public const string Delete = @"

DELETE FROM RolePermission

WHERE RoleId=@RoleId";



        public const string Insert = @"

INSERT INTO RolePermission

(

RoleId,

PermissionId,

CanView,

CanAdd,

CanEdit,

CanDelete,

CanExport

)

VALUES

(

@RoleId,

@PermissionId,

@CanView,

@CanAdd,

@CanEdit,

@CanDelete,

@CanExport

)";
    }
}
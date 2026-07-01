namespace GymManagement.Data.Queries
{
    public static class AccountQueries
    {
        public const string Login = @"
SELECT
    UserId,
    FullName,
    UserName,
    RoleId,
    PasswordHash
FROM UserMasters
WHERE UserName=@UserName
AND IsActive=1";

        public const string SetActive = @"
UPDATE UserMasters
SET LastLogin = GETDATE()
WHERE UserName=@UserName";

        public const string SetInactive = @"
-- No-op: Do not set IsActive = 0 on logout
SELECT 1";

        public const string GetProfile = @"
SELECT 
    u.UserId,
    u.FullName,
    u.UserName,
    u.Email,
    u.LastLogin,
    u.ProfilePhoto,
    r.RoleName
FROM UserMasters u
LEFT JOIN RoleMaster r ON u.RoleId = r.RoleId
WHERE u.UserName=@UserName";

        public const string GetPasswordHash = @"
SELECT PasswordHash FROM UserMasters WHERE UserName=@UserName";

        public const string UpdatePassword = @"
UPDATE UserMasters SET PasswordHash=@PasswordHash WHERE UserName=@UserName";

        public const string UpdateProfile = @"
UPDATE UserMasters SET FullName=@FullName, Email=@Email WHERE UserName=@UserName";

        public const string UpdateProfilePhoto = @"
UPDATE UserMasters SET ProfilePhoto=@ProfilePhoto WHERE UserName=@UserName";

        public const string GetProfilePhoto = @"
SELECT ProfilePhoto FROM UserMasters WHERE UserName=@UserName";
    }
}
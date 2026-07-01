namespace GymManagement.Data.Queries
{
    public static class UserQueries
    {
        public const string GetAll = @"
        SELECT
            U.*,
            R.RoleName
        FROM UserMasters U
        LEFT JOIN RoleMaster R
            ON U.RoleId = R.RoleId
        WHERE 1=1";

        public const string GetById = @"
        SELECT *
        FROM UserMasters
        WHERE UserId=@UserId";

        public const string Insert = @"
        INSERT INTO UserMasters
        (
            FullName,
            UserName,
            Email,
            PasswordHash,
            RoleId,
            IsActive,
            ProfilePhoto
        )
        VALUES
        (
            @FullName,
            @UserName,
            @Email,
            @PasswordHash,
            @RoleId,
            @IsActive,
            @ProfilePhoto
        )";

        public const string Update = @"
        UPDATE UserMasters
        SET
            FullName=@FullName,
            UserName=@UserName,
            Email=@Email,
            PasswordHash=@PasswordHash,
            RoleId=@RoleId,
            IsActive=@IsActive,
            ProfilePhoto=@ProfilePhoto
        WHERE UserId=@UserId";

        public const string Delete = @"
        DELETE FROM UserMasters
        WHERE UserId=@UserId";
    }
}
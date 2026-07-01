namespace GymManagement.Data.Queries
{
    public static class StaffQueries
    {
        public const string GetAll = @"
        SELECT S.*,
               R.RoleName
        FROM StaffMasters S
        LEFT JOIN RoleMaster R
            ON S.RoleId = R.RoleId
        ORDER BY S.StaffId DESC";

        public const string GetById =
        @"SELECT * FROM StaffMasters
          WHERE StaffId=@StaffId";

        public const string InsertWithExpertise = @"
        INSERT INTO StaffMasters
        (
            StaffName,
            Gender,
            MobileNo,
            Email,
            Designation,
            Specializations,
            ExperienceYears,
            Salary,
            JoiningDate,
            Address,
            IsActive,
            RoleId
        )
        VALUES
        (
            @StaffName,
            @Gender,
            @MobileNo,
            @Email,
            @Designation,
            @Specializations,
            @ExperienceYears,
            @Salary,
            @JoiningDate,
            @Address,
            @IsActive,
            @RoleId
        )";

        public const string UpdateWithExpertise = @"
        UPDATE StaffMasters
        SET
            StaffName=@StaffName,
            Gender=@Gender,
            MobileNo=@MobileNo,
            Email=@Email,
            Designation=@Designation,
            Specializations=@Specializations,
            ExperienceYears=@ExperienceYears,
            Salary=@Salary,
            JoiningDate=@JoiningDate,
            Address=@Address,
            IsActive=@IsActive,
            RoleId=@RoleId
        WHERE StaffId=@StaffId";

        public const string GetTrainersWithExpertise = @"
        SELECT
            S.StaffId,
            S.StaffName,
            S.Specializations,
            S.ExperienceYears
        FROM StaffMasters S
        INNER JOIN RoleMaster R ON S.RoleId = R.RoleId
        WHERE S.IsActive = 1 AND R.RoleName = 'Trainer'
        ORDER BY S.StaffName";

        public const string Insert = @"
        INSERT INTO StaffMasters
        (
            StaffName,
            Gender,
            MobileNo,
            Email,
            Designation,
            Salary,
            JoiningDate,
            Address,
            IsActive,
            RoleId
        )
        VALUES
        (
            @StaffName,
            @Gender,
            @MobileNo,
            @Email,
            @Designation,
            @Salary,
            @JoiningDate,
            @Address,
            @IsActive,
            @RoleId
        )";

        public const string Update = @"
        UPDATE StaffMasters
        SET
            StaffName=@StaffName,
            Gender=@Gender,
            MobileNo=@MobileNo,
            Email=@Email,
            Designation=@Designation,
            Salary=@Salary,
            JoiningDate=@JoiningDate,
            Address=@Address,
            IsActive=@IsActive,
            RoleId=@RoleId
        WHERE StaffId=@StaffId";

        public const string Delete =
        @"DELETE FROM StaffMasters
          WHERE StaffId=@StaffId";

        public const string InsertWithProfile = @"
        INSERT INTO StaffMasters
        (
            StaffCode, StaffName, Gender, MobileNo, Email, Designation,
            Specializations, ExperienceYears, Salary, JoiningDate, Address,
            IsActive, RoleId, BankName, BankAccountNo, IfscCode
        )
        VALUES
        (
            @StaffCode, @StaffName, @Gender, @MobileNo, @Email, @Designation,
            @Specializations, @ExperienceYears, @Salary, @JoiningDate, @Address,
            @IsActive, @RoleId, @BankName, @BankAccountNo, @IfscCode
        );
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public const string UpdateWithProfile = @"
        UPDATE StaffMasters
        SET
            StaffCode=@StaffCode,
            StaffName=@StaffName,
            Gender=@Gender,
            MobileNo=@MobileNo,
            Email=@Email,
            Designation=@Designation,
            Specializations=@Specializations,
            ExperienceYears=@ExperienceYears,
            Salary=@Salary,
            JoiningDate=@JoiningDate,
            Address=@Address,
            IsActive=@IsActive,
            RoleId=@RoleId,
            BankName=@BankName,
            BankAccountNo=@BankAccountNo,
            IfscCode=@IfscCode
        WHERE StaffId=@StaffId";

        public const string IsStaffCodeTaken = @"
        SELECT COUNT(*) FROM StaffMasters
        WHERE UPPER(LTRIM(RTRIM(StaffCode))) = UPPER(LTRIM(RTRIM(@StaffCode)))
          AND (@ExcludeStaffId IS NULL OR StaffId <> @ExcludeStaffId)";

        public const string GetByStaffCode = @"
        SELECT TOP 1 S.*, R.RoleName
        FROM StaffMasters S
        LEFT JOIN RoleMaster R ON S.RoleId = R.RoleId
        WHERE UPPER(LTRIM(RTRIM(S.StaffCode))) = UPPER(LTRIM(RTRIM(@StaffCode)))";
    }
}
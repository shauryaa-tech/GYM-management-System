namespace GymManagement.Data.Queries
{
    public static class VendorQueries
    {
        public const string GetAll = @"
        SELECT * FROM VendorMasters
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM VendorMasters
        WHERE VendorId=@VendorId";

        public const string Insert = @"
        INSERT INTO VendorMasters
        (
            VendorName,
            ContactPerson,
            MobileNo,
            Email,
            Address,
            GSTNo,
            IsActive
        )
        VALUES
        (
            @VendorName,
            @ContactPerson,
            @MobileNo,
            @Email,
            @Address,
            @GSTNo,
            @IsActive
        )";

        public const string Update = @"
        UPDATE VendorMasters
        SET
            VendorName=@VendorName,
            ContactPerson=@ContactPerson,
            MobileNo=@MobileNo,
            Email=@Email,
            Address=@Address,
            GSTNo=@GSTNo,
            IsActive=@IsActive
        WHERE VendorId=@VendorId";

        public const string Delete =
            "DELETE FROM VendorMasters WHERE VendorId=@VendorId";
    }
}

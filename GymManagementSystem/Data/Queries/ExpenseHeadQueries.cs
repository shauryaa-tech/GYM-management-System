namespace GymManagement.Data.Queries
{
    public static class ExpenseHeadQueries
    {
        public const string GetAll = @"
        SELECT * FROM ExpenseHeadMasters
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM ExpenseHeadMasters
        WHERE ExpenseHeadId=@ExpenseHeadId";

        public const string Insert = @"
        INSERT INTO ExpenseHeadMasters
        (
            HeadName,
            Description,
            IsActive
        )
        VALUES
        (
            @HeadName,
            @Description,
            @IsActive
        )";

        public const string Update = @"
        UPDATE ExpenseHeadMasters
        SET
            HeadName=@HeadName,
            Description=@Description,
            IsActive=@IsActive
        WHERE ExpenseHeadId=@ExpenseHeadId";

        public const string Delete =
            "DELETE FROM ExpenseHeadMasters WHERE ExpenseHeadId=@ExpenseHeadId";
    }
}

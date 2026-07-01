namespace GymManagement.Data.Queries
{
    public static class ClassQueries
    {
        public const string GetAll = @"
        SELECT 
            C.*, 
            S.StaffName AS TrainerName 
        FROM ClassMasters C
        LEFT JOIN StaffMasters S ON C.TrainerId = S.StaffId
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM ClassMasters
        WHERE ClassId=@ClassId";

        public const string Insert = @"
        INSERT INTO ClassMasters
        (
            ClassName,
            TrainerId,
            Schedule,
            StartTime,
            EndTime,
            MaxCapacity,
            Amount,
            IsActive
        )
        VALUES
        (
            @ClassName,
            @TrainerId,
            @Schedule,
            @StartTime,
            @EndTime,
            @MaxCapacity,
            @Amount,
            @IsActive
        )";

        public const string Update = @"
        UPDATE ClassMasters
        SET
            ClassName=@ClassName,
            TrainerId=@TrainerId,
            Schedule=@Schedule,
            StartTime=@StartTime,
            EndTime=@EndTime,
            MaxCapacity=@MaxCapacity,
            Amount=@Amount,
            IsActive=@IsActive
        WHERE ClassId=@ClassId";

        public const string Delete =
            "DELETE FROM ClassMasters WHERE ClassId=@ClassId";
    }
}

namespace GymManagement.Data.Queries
{
    public static class DietQueries
    {
        public const string GetAll = @"
        SELECT * FROM DietMasters 
        WHERE 1=1";

        public const string GetById = @"
        SELECT * FROM DietMasters
        WHERE DietId=@DietId";

        public const string Insert = @"
        INSERT INTO DietMasters
        (
            DietName,
            Category,
            MealType,
            Calories,
            Protein,
            Carbs,
            Fat,
            Description,
            IsActive
        )
        VALUES
        (
            @DietName,
            @Category,
            @MealType,
            @Calories,
            @Protein,
            @Carbs,
            @Fat,
            @Description,
            @IsActive
        )";

        public const string Update = @"
        UPDATE DietMasters
        SET
            DietName=@DietName,
            Category=@Category,
            MealType=@MealType,
            Calories=@Calories,
            Protein=@Protein,
            Carbs=@Carbs,
            Fat=@Fat,
            Description=@Description,
            IsActive=@IsActive
        WHERE DietId=@DietId";

        public const string Delete =
            "DELETE FROM DietMasters WHERE DietId=@DietId";
    }
}

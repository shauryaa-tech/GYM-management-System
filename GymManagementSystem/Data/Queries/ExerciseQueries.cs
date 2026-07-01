namespace GymManagement.Data.Queries
{
    public static class ExerciseQueries
    {
        public const string GetAll =
            @"SELECT * FROM ExerciseMaster";

        public const string GetById =
            @"SELECT * FROM ExerciseMaster
              WHERE ExerciseId=@ExerciseId";

        public const string Insert =
            @"INSERT INTO ExerciseMaster
            (
                ExerciseName,
                MuscleGroup,
                DifficultyLevel,
                CaloriesBurn,
                Description,
                Status
            )
            VALUES
            (
                @ExerciseName,
                @MuscleGroup,
                @DifficultyLevel,
                @CaloriesBurn,
                @Description,
                @Status
            )";

        public const string Update =
            @"UPDATE ExerciseMaster
              SET
                ExerciseName=@ExerciseName,
                MuscleGroup=@MuscleGroup,
                DifficultyLevel=@DifficultyLevel,
                CaloriesBurn=@CaloriesBurn,
                Description=@Description,
                Status=@Status
              WHERE ExerciseId=@ExerciseId";

        public const string Delete =
            @"DELETE FROM ExerciseMaster
              WHERE ExerciseId=@ExerciseId";
    }
}
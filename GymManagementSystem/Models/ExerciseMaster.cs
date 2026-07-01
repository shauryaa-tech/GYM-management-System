namespace GymManagement.Models
{
    public class ExerciseMaster
    {
        public int ExerciseId { get; set; }

        public string ExerciseName { get; set; } = "";

        public string MuscleGroup { get; set; } = "";

        public string DifficultyLevel { get; set; } = "";

        public int CaloriesBurn { get; set; }

        public string Description { get; set; } = "";

        public bool Status { get; set; }
    }
}
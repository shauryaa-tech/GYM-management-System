using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Repositories
{
    public class ExerciseRepository
    {
        private readonly DbHelper _db;

        public ExerciseRepository(DbHelper db)
        {
            _db = db;
        }

        public List<ExerciseMaster> GetAll(
            string? search,
            bool? status)
        {
            List<ExerciseMaster> list = new();

            using (SqlConnection con = _db.GetConnection())
            {
                con.Open();

                string query = @"
                    SELECT *
                    FROM ExerciseMaster
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += @"
                        AND
                        (
                            ExerciseName LIKE @Search
                            OR MuscleGroup LIKE @Search
                            OR DifficultyLevel LIKE @Search
                        )";
                }

                if (status != null)
                {
                    query += " AND Status=@Status";
                }

                query += " ORDER BY ExerciseId DESC";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");
                }

                if (status != null)
                {
                    cmd.Parameters.AddWithValue(
                        "@Status",
                        status);
                }

                SqlDataReader dr =
                    cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new ExerciseMaster
                    {
                        ExerciseId =
                            Convert.ToInt32(dr["ExerciseId"]),

                        ExerciseName =
                            dr["ExerciseName"].ToString()!,

                        MuscleGroup =
                            dr["MuscleGroup"].ToString()!,

                        DifficultyLevel =
                            dr["DifficultyLevel"].ToString()!,

                        CaloriesBurn =
                            Convert.ToInt32(dr["CaloriesBurn"]),

                        Description =
                            dr["Description"].ToString()!,

                        Status =
                            Convert.ToBoolean(dr["Status"])
                    });
                }
            }


            return list;
        }

        public void Insert(ExerciseMaster model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    ExerciseQueries.Insert,
                    con);

            cmd.Parameters.AddWithValue(
                "@ExerciseName",
                model.ExerciseName);

            cmd.Parameters.AddWithValue(
                "@MuscleGroup",
                model.MuscleGroup);

            cmd.Parameters.AddWithValue(
                "@DifficultyLevel",
                model.DifficultyLevel);

            cmd.Parameters.AddWithValue(
                "@CaloriesBurn",
                model.CaloriesBurn);

            cmd.Parameters.AddWithValue(
                "@Description",
                model.Description ?? "");

            cmd.Parameters.AddWithValue(
                "@Status",
                model.Status);

            cmd.ExecuteNonQuery();
        }

        public ExerciseMaster GetById(int id)
        {
            ExerciseMaster exercise = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    ExerciseQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue(
                "@ExerciseId",
                id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                exercise.ExerciseId =
                    Convert.ToInt32(dr["ExerciseId"]);

                exercise.ExerciseName =
                    dr["ExerciseName"].ToString()!;

                exercise.MuscleGroup =
                    dr["MuscleGroup"].ToString()!;

                exercise.DifficultyLevel =
                    dr["DifficultyLevel"].ToString()!;

                exercise.CaloriesBurn =
                    Convert.ToInt32(dr["CaloriesBurn"]);

                exercise.Description =
                    dr["Description"].ToString()!;

                exercise.Status =
                    Convert.ToBoolean(dr["Status"]);
            }

            return exercise;
        }

        public void Update(ExerciseMaster model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    ExerciseQueries.Update,
                    con);

            cmd.Parameters.AddWithValue(
                "@ExerciseId",
                model.ExerciseId);

            cmd.Parameters.AddWithValue(
                "@ExerciseName",
                model.ExerciseName);

            cmd.Parameters.AddWithValue(
                "@MuscleGroup",
                model.MuscleGroup);

            cmd.Parameters.AddWithValue(
                "@DifficultyLevel",
                model.DifficultyLevel);

            cmd.Parameters.AddWithValue(
                "@CaloriesBurn",
                model.CaloriesBurn);

            cmd.Parameters.AddWithValue(
                "@Description",
                model.Description ?? "");

            cmd.Parameters.AddWithValue(
                "@Status",
                model.Status);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    ExerciseQueries.Delete,
                    con);

            cmd.Parameters.AddWithValue(
                "@ExerciseId",
                id);

            cmd.ExecuteNonQuery();
        }

    }
}
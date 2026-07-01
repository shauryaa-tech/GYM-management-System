using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class WorkoutPlanRepository
    {
        private readonly DbHelper _db;

        public WorkoutPlanRepository(DbHelper db)
        {
            _db = db;
        }

        public List<WorkoutPlan> GetAll(string? search)
        {
            List<WorkoutPlan> plans = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = WorkoutPlanQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (WP.PlanName LIKE @Search OR M.MemberName LIKE @Search OR M.MemberCode LIKE @Search)";
            }

            query += " ORDER BY WP.PlanId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                plans.Add(new WorkoutPlan
                {
                    PlanId = Convert.ToInt32(dr["PlanId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    TrainerId = dr["TrainerId"] == DBNull.Value ? null : Convert.ToInt32(dr["TrainerId"]),
                    PlanName = dr["PlanName"]?.ToString() ?? "",
                    StartDate = Convert.ToDateTime(dr["StartDate"]),
                    EndDate = Convert.ToDateTime(dr["EndDate"]),
                    Goals = dr["Goals"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    TrainerName = dr["TrainerName"]?.ToString()
                });
            }

            return plans;
        }

        public void Insert(WorkoutPlan model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(WorkoutPlanQueries.Insert, con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@TrainerId", (object?)model.TrainerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PlanName", model.PlanName);
            cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
            cmd.Parameters.AddWithValue("@Goals", (object?)model.Goals ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public WorkoutPlan GetById(int id)
        {
            WorkoutPlan model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(WorkoutPlanQueries.GetById, con);
            cmd.Parameters.AddWithValue("@PlanId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.PlanId = Convert.ToInt32(dr["PlanId"]);
                model.MemberId = Convert.ToInt32(dr["MemberId"]);
                model.TrainerId = dr["TrainerId"] == DBNull.Value ? null : Convert.ToInt32(dr["TrainerId"]);
                model.PlanName = dr["PlanName"]?.ToString() ?? "";
                model.StartDate = Convert.ToDateTime(dr["StartDate"]);
                model.EndDate = Convert.ToDateTime(dr["EndDate"]);
                model.Goals = dr["Goals"]?.ToString();
                model.Remarks = dr["Remarks"]?.ToString();
            }

            return model;
        }

        public void Update(WorkoutPlan model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(WorkoutPlanQueries.Update, con);

            cmd.Parameters.AddWithValue("@PlanId", model.PlanId);
            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@TrainerId", (object?)model.TrainerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PlanName", model.PlanName);
            cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
            cmd.Parameters.AddWithValue("@Goals", (object?)model.Goals ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(WorkoutPlanQueries.Delete, con);
            cmd.Parameters.AddWithValue("@PlanId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

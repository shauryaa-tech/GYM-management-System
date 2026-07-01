using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class DietPlanRepository
    {
        private readonly DbHelper _db;

        public DietPlanRepository(DbHelper db)
        {
            _db = db;
        }

        public List<DietPlan> GetAll(string? search)
        {
            List<DietPlan> plans = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = DietPlanQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (DP.PlanName LIKE @Search OR M.MemberName LIKE @Search OR M.MemberCode LIKE @Search)";
            }

            query += " ORDER BY DP.DietPlanId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                plans.Add(new DietPlan
                {
                    DietPlanId = Convert.ToInt32(dr["DietPlanId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    PlanName = dr["PlanName"]?.ToString() ?? "",
                    StartDate = Convert.ToDateTime(dr["StartDate"]),
                    EndDate = Convert.ToDateTime(dr["EndDate"]),
                    CalorieTarget = dr["CalorieTarget"] == DBNull.Value ? null : Convert.ToDecimal(dr["CalorieTarget"]),
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString()
                });
            }

            return plans;
        }

        public void Insert(DietPlan model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietPlanQueries.Insert, con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@PlanName", model.PlanName);
            cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
            cmd.Parameters.AddWithValue("@CalorieTarget", (object?)model.CalorieTarget ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public DietPlan GetById(int id)
        {
            DietPlan model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietPlanQueries.GetById, con);
            cmd.Parameters.AddWithValue("@DietPlanId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.DietPlanId = Convert.ToInt32(dr["DietPlanId"]);
                model.MemberId = Convert.ToInt32(dr["MemberId"]);
                model.PlanName = dr["PlanName"]?.ToString() ?? "";
                model.StartDate = Convert.ToDateTime(dr["StartDate"]);
                model.EndDate = Convert.ToDateTime(dr["EndDate"]);
                model.CalorieTarget = dr["CalorieTarget"] == DBNull.Value ? null : Convert.ToDecimal(dr["CalorieTarget"]);
                model.Remarks = dr["Remarks"]?.ToString();
            }

            return model;
        }

        public void Update(DietPlan model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietPlanQueries.Update, con);

            cmd.Parameters.AddWithValue("@DietPlanId", model.DietPlanId);
            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@PlanName", model.PlanName);
            cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
            cmd.Parameters.AddWithValue("@CalorieTarget", (object?)model.CalorieTarget ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietPlanQueries.Delete, con);
            cmd.Parameters.AddWithValue("@DietPlanId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

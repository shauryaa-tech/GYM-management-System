using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class MembershipRepository
    {
        private readonly DbHelper _db;

        public MembershipRepository(DbHelper db)
        {
            _db = db;
        }

        public List<MembershipPlanMaster> GetAll(
            string? search,
            bool? status)
        {
            List<MembershipPlanMaster> plans = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            string query =
                @"SELECT * FROM MembershipPlanMasters
                  WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
                query += " AND PlanName LIKE @Search";

            if (status.HasValue)
                query += " AND IsActive=@Status";

            query += " ORDER BY PlanId DESC";

            SqlCommand cmd =
                new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" + search + "%");

            if (status.HasValue)
                cmd.Parameters.AddWithValue(
                    "@Status",
                    status.Value);

            SqlDataReader dr =
                cmd.ExecuteReader();

            while (dr.Read())
            {
                plans.Add(new MembershipPlanMaster
                {
                    PlanId =
                        Convert.ToInt32(dr["PlanId"]),
                    PlanName =
                        dr["PlanName"].ToString(),
                    DurationMonths =
                        Convert.ToInt32(dr["DurationMonths"]),
                    Amount =
                        Convert.ToDecimal(dr["Amount"]),
                    JoiningFee =
                        Convert.ToDecimal(dr["JoiningFee"]),
                    Description =
                        dr["Description"].ToString(),
                    IsActive =
                        Convert.ToBoolean(dr["IsActive"])
                });
            }

            return plans;
        }

        public void Insert(MembershipPlanMaster plan)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembershipQueries.Insert,
                    con);

            cmd.Parameters.AddWithValue("@PlanName", plan.PlanName);
            cmd.Parameters.AddWithValue("@DurationMonths", plan.DurationMonths);
            cmd.Parameters.AddWithValue("@Amount", plan.Amount);
            cmd.Parameters.AddWithValue("@JoiningFee", plan.JoiningFee);
            cmd.Parameters.AddWithValue("@Description", plan.Description ?? "");
            cmd.Parameters.AddWithValue("@IsActive", plan.IsActive);

            cmd.ExecuteNonQuery();
        }

        public MembershipPlanMaster GetById(int id)
        {
            MembershipPlanMaster plan = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembershipQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue("@PlanId", id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                plan.PlanId =
                    Convert.ToInt32(dr["PlanId"]);

                plan.PlanName =
                    dr["PlanName"].ToString();

                plan.DurationMonths =
                    Convert.ToInt32(dr["DurationMonths"]);

                plan.Amount =
                    Convert.ToDecimal(dr["Amount"]);

                plan.JoiningFee =
                    Convert.ToDecimal(dr["JoiningFee"]);

                plan.Description =
                    dr["Description"].ToString();

                plan.IsActive =
                    Convert.ToBoolean(dr["IsActive"]);
            }

            return plan;
        }

        public void Update(MembershipPlanMaster plan)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembershipQueries.Update,
                    con);

            cmd.Parameters.AddWithValue("@PlanId", plan.PlanId);
            cmd.Parameters.AddWithValue("@PlanName", plan.PlanName);
            cmd.Parameters.AddWithValue("@DurationMonths", plan.DurationMonths);
            cmd.Parameters.AddWithValue("@Amount", plan.Amount);
            cmd.Parameters.AddWithValue("@JoiningFee", plan.JoiningFee);
            cmd.Parameters.AddWithValue("@Description", plan.Description ?? "");
            cmd.Parameters.AddWithValue("@IsActive", plan.IsActive);


            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembershipQueries.Delete,
                    con);

            cmd.Parameters.AddWithValue("@PlanId", id);

            cmd.ExecuteNonQuery();
        }
    }
}
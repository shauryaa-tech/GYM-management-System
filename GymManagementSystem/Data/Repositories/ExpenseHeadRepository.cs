using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class ExpenseHeadRepository
    {
        private readonly DbHelper _db;

        public ExpenseHeadRepository(DbHelper db)
        {
            _db = db;
        }

        public List<ExpenseHeadMaster> GetAll(string? search)
        {
            List<ExpenseHeadMaster> heads = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = ExpenseHeadQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (HeadName LIKE @Search OR Description LIKE @Search)";
            }

            query += " ORDER BY ExpenseHeadId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                heads.Add(new ExpenseHeadMaster
                {
                    ExpenseHeadId = Convert.ToInt32(dr["ExpenseHeadId"]),
                    HeadName = dr["HeadName"]?.ToString() ?? "",
                    Description = dr["Description"]?.ToString(),
                    IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                });
            }

            return heads;
        }

        public void Insert(ExpenseHeadMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseHeadQueries.Insert, con);

            cmd.Parameters.AddWithValue("@HeadName", model.HeadName);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public ExpenseHeadMaster GetById(int id)
        {
            ExpenseHeadMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseHeadQueries.GetById, con);
            cmd.Parameters.AddWithValue("@ExpenseHeadId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.ExpenseHeadId = Convert.ToInt32(dr["ExpenseHeadId"]);
                model.HeadName = dr["HeadName"]?.ToString() ?? "";
                model.Description = dr["Description"]?.ToString();
                model.IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Update(ExpenseHeadMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseHeadQueries.Update, con);

            cmd.Parameters.AddWithValue("@ExpenseHeadId", model.ExpenseHeadId);
            cmd.Parameters.AddWithValue("@HeadName", model.HeadName);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseHeadQueries.Delete, con);
            cmd.Parameters.AddWithValue("@ExpenseHeadId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

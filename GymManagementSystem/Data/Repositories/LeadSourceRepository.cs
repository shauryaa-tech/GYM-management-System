using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class LeadSourceRepository
    {
        private readonly DbHelper _db;

        public LeadSourceRepository(DbHelper db)
        {
            _db = db;
        }

        private string GenerateSourceCode()
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(MAX(LeadSourceId),0)+1 FROM LeadSourceMasters",
                con);

            int nextId = Convert.ToInt32(cmd.ExecuteScalar());

            return "LS" + nextId.ToString("000");
        }

        public List<LeadSourceMaster> GetAll(string? search)
        {
            List<LeadSourceMaster> list = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = LeadSourceQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += @" AND
                (
                    SourceCode LIKE @Search
                    OR SourceName LIKE @Search
                    OR Description LIKE @Search
                )";
            }

            query += " ORDER BY DisplayOrder, SourceName";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new LeadSourceMaster
                {
                    LeadSourceId = Convert.ToInt32(dr["LeadSourceId"]),
                    SourceCode = dr["SourceCode"].ToString() ?? "",
                    SourceName = dr["SourceName"].ToString() ?? "",
                    Description = dr["Description"].ToString() ?? "",
                    DisplayOrder = Convert.ToInt32(dr["DisplayOrder"]),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                });
            }

            return list;
        }

        public void Insert(LeadSourceMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            model.SourceCode = GenerateSourceCode();

            SqlCommand cmd = new SqlCommand(
                LeadSourceQueries.Insert,
                con);

            cmd.Parameters.AddWithValue("@SourceCode", model.SourceCode);
            cmd.Parameters.AddWithValue("@SourceName", model.SourceName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
            cmd.Parameters.AddWithValue("@DisplayOrder", model.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public LeadSourceMaster GetById(int id)
        {
            LeadSourceMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(
                LeadSourceQueries.GetById,
                con);

            cmd.Parameters.AddWithValue("@LeadSourceId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.LeadSourceId = Convert.ToInt32(dr["LeadSourceId"]);
                model.SourceCode = dr["SourceCode"].ToString() ?? "";
                model.SourceName = dr["SourceName"].ToString() ?? "";
                model.Description = dr["Description"].ToString() ?? "";
                model.DisplayOrder = Convert.ToInt32(dr["DisplayOrder"]);
                model.IsActive = Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Update(LeadSourceMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(
                LeadSourceQueries.Update,
                con);

            cmd.Parameters.AddWithValue("@LeadSourceId", model.LeadSourceId);
            cmd.Parameters.AddWithValue("@SourceName", model.SourceName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
            cmd.Parameters.AddWithValue("@DisplayOrder", model.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(
                LeadSourceQueries.Delete,
                con);

            cmd.Parameters.AddWithValue("@LeadSourceId", id);

            cmd.ExecuteNonQuery();
        }
    }
}
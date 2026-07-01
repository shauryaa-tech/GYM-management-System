using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class RoleRepository
    {
        private readonly DbHelper _db;

        public RoleRepository(DbHelper db)
        {
            _db = db;
        }

        public List<RoleMaster> GetAll()
        {
            List<RoleMaster> list = new();

            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT *
                FROM RoleMaster
                ORDER BY RoleName", con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new RoleMaster
                {
                    RoleId = Convert.ToInt64(dr["RoleId"]),
                    RoleName = dr["RoleName"].ToString() ?? "",
                    Description = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                });
            }

            return list;
        }

        public RoleMaster GetById(long id)
        {
            RoleMaster role = new();

            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT *
                FROM RoleMaster
                WHERE RoleId=@RoleId", con);

            cmd.Parameters.AddWithValue("@RoleId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                role.RoleId = Convert.ToInt64(dr["RoleId"]);
                role.RoleName = dr["RoleName"].ToString() ?? "";
                role.Description = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString();
                role.IsActive = Convert.ToBoolean(dr["IsActive"]);
            }

            return role;
        }

        public void Insert(RoleMaster role)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO RoleMaster
                (
                RoleName,
                Description,
                IsActive
                )
                VALUES
                (
                @RoleName,
                @Description,
                @IsActive
                )", con);

            cmd.Parameters.AddWithValue("@RoleName", role.RoleName);

            cmd.Parameters.AddWithValue("@Description",
                (object?)role.Description ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@IsActive",
                role.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Update(RoleMaster role)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(@"
                UPDATE RoleMaster
                SET
                RoleName=@RoleName,
                Description=@Description,
                IsActive=@IsActive
                WHERE RoleId=@RoleId", con);

            cmd.Parameters.AddWithValue("@RoleId", role.RoleId);

            cmd.Parameters.AddWithValue("@RoleName", role.RoleName);

            cmd.Parameters.AddWithValue("@Description",
                (object?)role.Description ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@IsActive",
                role.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(@"
                DELETE FROM RoleMaster
                WHERE RoleId=@RoleId", con);

            cmd.Parameters.AddWithValue("@RoleId", id);

            cmd.ExecuteNonQuery();
        }
    }
}
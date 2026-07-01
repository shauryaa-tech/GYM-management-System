using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class UserRepository
    {
        private readonly DbHelper _db;

        public UserRepository(DbHelper db)
        {
            _db = db;
        }

        public List<UserMaster> GetAll(string? search)
        {
            List<UserMaster> users = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = UserQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (U.FullName LIKE @Search OR U.UserName LIKE @Search)";
            }

            query += " ORDER BY U.UserId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                users.Add(new UserMaster
                {
                    UserId = Convert.ToInt32(dr["UserId"]),
                    FullName = dr["FullName"]?.ToString() ?? "",
                    UserName = dr["UserName"]?.ToString() ?? "",
                    RoleId = Convert.ToInt32(dr["RoleId"]),
                    RoleName = dr["RoleName"]?.ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"]),

                    LastLogin =
                        dr["LastLogin"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["LastLogin"]),

                    ProfilePhoto =
                        dr["ProfilePhoto"] == DBNull.Value
                        ? null
                        : dr["ProfilePhoto"].ToString()
                });
            }

            return users;
        }

        public bool IsUserNameExists(string userName, int userId = 0)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            string sql = @"SELECT COUNT(*)
                   FROM UserMasters
                   WHERE UserName=@UserName
                   AND UserId<>@UserId";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@UserId", userId);

            return (int)cmd.ExecuteScalar() > 0;
        }

        public bool IsEmailExists(string email, int userId = 0)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            string sql = @"SELECT COUNT(*)
                   FROM UserMasters
                   WHERE Email=@Email
                   AND UserId<>@UserId";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@UserId", userId);

            return (int)cmd.ExecuteScalar() > 0;
        }

        public void Insert(UserMaster model)
        {
            if (IsUserNameExists(model.UserName))
                throw new Exception("Username already exists.");

            if (IsEmailExists(model.Email))
                throw new Exception("Email already exists.");

            using SqlConnection con = _db.GetConnection();
            con.Open();

            using SqlCommand cmd = new SqlCommand(UserQueries.Insert, con);

            cmd.Parameters.AddWithValue("@FullName", model.FullName);
            cmd.Parameters.AddWithValue("@UserName", model.UserName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
            cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
            cmd.Parameters.AddWithValue("@ProfilePhoto",
                (object?)model.ProfilePhoto ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public UserMaster GetById(int id)
        {
            UserMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd =
                new SqlCommand(UserQueries.GetById, con);

            cmd.Parameters.AddWithValue(
                "@UserId",
                id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.UserId =
                    Convert.ToInt32(dr["UserId"]);

                model.FullName =
                    dr["FullName"]?.ToString() ?? "";

                model.UserName =
                    dr["UserName"]?.ToString() ?? "";

                model.PasswordHash =
                    dr["PasswordHash"]?.ToString() ?? "";

                model.Email =
                    dr["Email"]?.ToString() ?? "";

                model.RoleId =
                    Convert.ToInt32(dr["RoleId"]);

                model.IsActive =
                    Convert.ToBoolean(dr["IsActive"]);

                model.LastLogin =
                    dr["LastLogin"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(dr["LastLogin"]);

                model.ProfilePhoto =
                    dr["ProfilePhoto"] == DBNull.Value
                    ? null
                    : dr["ProfilePhoto"].ToString();
            }

            return model;
        }

        public void Update(UserMaster model)
        {
            if (IsUserNameExists(model.UserName, model.UserId))
                throw new Exception("Username already exists.");

            if (IsEmailExists(model.Email, model.UserId))
                throw new Exception("Email already exists.");

            using SqlConnection con = _db.GetConnection();
            con.Open();

            using SqlCommand cmd = new SqlCommand(UserQueries.Update, con);

            cmd.Parameters.AddWithValue("@UserId", model.UserId);
            cmd.Parameters.AddWithValue("@FullName", model.FullName);
            cmd.Parameters.AddWithValue("@UserName", model.UserName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
            cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
            cmd.Parameters.AddWithValue("@ProfilePhoto",
                (object?)model.ProfilePhoto ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }


        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd =
                new SqlCommand(UserQueries.Delete, con);

            cmd.Parameters.AddWithValue(
                "@UserId",
                id);

            cmd.ExecuteNonQuery();
        }
    }
}
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;
using BCrypt.Net;
using GymManagement.ViewModels;

namespace GymManagement.Data.Repositories
{
    public class AccountRepository
    {
        private readonly DbHelper _db;

        public AccountRepository(DbHelper db)
        {
            _db = db;
        }

        public (bool Success, string FullName, int RoleId) Login(string userName, string password)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AccountQueries.Login, con);

            cmd.Parameters.AddWithValue("@UserName", userName);

            SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read())
                return (false, "", 0);

            string fullName = dr["FullName"].ToString() ?? "";

            string hash = dr["PasswordHash"].ToString() ?? "";

            int roleId = Convert.ToInt32(dr["RoleId"]);

            dr.Close();

            if (!BCrypt.Net.BCrypt.Verify(password, hash))
                return (false, "", 0);

            SqlCommand activeCmd = new SqlCommand(AccountQueries.SetActive, con);

            activeCmd.Parameters.AddWithValue("@UserName", userName);

            activeCmd.ExecuteNonQuery();

            return (true, fullName, roleId);
        }

        public void LogoutUser(string userName)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    AccountQueries.SetInactive,
                    con);

            cmd.Parameters.AddWithValue(
                "@UserName",
                userName);

            cmd.ExecuteNonQuery();
        }

        public ProfileViewModel GetProfile(string userName)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AccountQueries.GetProfile, con);
            cmd.Parameters.AddWithValue("@UserName", userName);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                var profile = new ProfileViewModel
                {
                    UserId = dr["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["UserId"]),
                    FullName = dr["FullName"]?.ToString() ?? "",
                    UserName = dr["UserName"]?.ToString() ?? "",
                    RoleName = dr["RoleName"]?.ToString() ?? ""
                };

                // Handle optional columns that might not exist yet
                try { profile.Email = dr["Email"]?.ToString() ?? ""; } catch { profile.Email = ""; }
                try { profile.LastLogin = dr["LastLogin"] == DBNull.Value ? null : Convert.ToDateTime(dr["LastLogin"]); } catch { profile.LastLogin = null; }
                try { profile.ProfilePhoto = dr["ProfilePhoto"]?.ToString(); } catch { profile.ProfilePhoto = null; }

                return profile;
            }

            return null!;
        }

        public bool ChangePassword(string userName, string currentPassword, string newPassword)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AccountQueries.GetPasswordHash, con);
            cmd.Parameters.AddWithValue("@UserName", userName);

            var hashObj = cmd.ExecuteScalar();
            if (hashObj == null) return false;

            string hash = hashObj.ToString()!;
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, hash))
                return false;

            string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            SqlCommand updateCmd = new SqlCommand(AccountQueries.UpdatePassword, con);
            updateCmd.Parameters.AddWithValue("@UserName", userName);
            updateCmd.Parameters.AddWithValue("@PasswordHash", newHash);

            return updateCmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateProfile(string userName, string fullName, string email)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AccountQueries.UpdateProfile, con);
            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@FullName", fullName);
            cmd.Parameters.AddWithValue("@Email", email);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateProfilePhoto(string userName, string profilePhoto)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AccountQueries.UpdateProfilePhoto, con);
            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@ProfilePhoto", profilePhoto);

            return cmd.ExecuteNonQuery() > 0;
        }

        public string GetProfilePhoto(string userName)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AccountQueries.GetProfilePhoto, con);
            cmd.Parameters.AddWithValue("@UserName", userName);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }
}
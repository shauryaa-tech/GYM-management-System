using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class VendorRepository
    {
        private readonly DbHelper _db;

        public VendorRepository(DbHelper db)
        {
            _db = db;
        }

        public List<VendorMaster> GetAll(string? search)
        {
            List<VendorMaster> vendors = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = VendorQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (VendorName LIKE @Search OR ContactPerson LIKE @Search OR MobileNo LIKE @Search OR GSTNo LIKE @Search)";
            }

            query += " ORDER BY VendorId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                vendors.Add(new VendorMaster
                {
                    VendorId = Convert.ToInt32(dr["VendorId"]),
                    VendorName = dr["VendorName"]?.ToString() ?? "",
                    ContactPerson = dr["ContactPerson"]?.ToString(),
                    MobileNo = dr["MobileNo"]?.ToString(),
                    Email = dr["Email"]?.ToString(),
                    Address = dr["Address"]?.ToString(),
                    GSTNo = dr["GSTNo"]?.ToString(),
                    IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                });
            }

            return vendors;
        }

        public void Insert(VendorMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(VendorQueries.Insert, con);

            cmd.Parameters.AddWithValue("@VendorName", model.VendorName);
            cmd.Parameters.AddWithValue("@ContactPerson", (object?)model.ContactPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobileNo", (object?)model.MobileNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GSTNo", (object?)model.GSTNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public VendorMaster GetById(int id)
        {
            VendorMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(VendorQueries.GetById, con);
            cmd.Parameters.AddWithValue("@VendorId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.VendorId = Convert.ToInt32(dr["VendorId"]);
                model.VendorName = dr["VendorName"]?.ToString() ?? "";
                model.ContactPerson = dr["ContactPerson"]?.ToString();
                model.MobileNo = dr["MobileNo"]?.ToString();
                model.Email = dr["Email"]?.ToString();
                model.Address = dr["Address"]?.ToString();
                model.GSTNo = dr["GSTNo"]?.ToString();
                model.IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Update(VendorMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(VendorQueries.Update, con);

            cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
            cmd.Parameters.AddWithValue("@VendorName", model.VendorName);
            cmd.Parameters.AddWithValue("@ContactPerson", (object?)model.ContactPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobileNo", (object?)model.MobileNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GSTNo", (object?)model.GSTNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(VendorQueries.Delete, con);
            cmd.Parameters.AddWithValue("@VendorId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

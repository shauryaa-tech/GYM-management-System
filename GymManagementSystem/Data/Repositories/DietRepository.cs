using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class DietRepository
    {
        private readonly DbHelper _db;

        public DietRepository(DbHelper db)
        {
            _db = db;
        }

        public List<DietMaster> GetAll(string? search, string? category)
        {
            List<DietMaster> diets = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = DietQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (DietName LIKE @Search OR Description LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query += " AND Category=@Category";
            }

            query += " ORDER BY DietId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                cmd.Parameters.AddWithValue("@Category", category);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                diets.Add(new DietMaster
                {
                    DietId = Convert.ToInt32(dr["DietId"]),
                    DietName = dr["DietName"]?.ToString() ?? "",
                    Category = dr["Category"]?.ToString() ?? "",
                    MealType = dr["MealType"]?.ToString() ?? "",
                    Calories = dr["Calories"] == DBNull.Value ? null : Convert.ToDecimal(dr["Calories"]),
                    Protein = dr["Protein"] == DBNull.Value ? null : Convert.ToDecimal(dr["Protein"]),
                    Carbs = dr["Carbs"] == DBNull.Value ? null : Convert.ToDecimal(dr["Carbs"]),
                    Fat = dr["Fat"] == DBNull.Value ? null : Convert.ToDecimal(dr["Fat"]),
                    Description = dr["Description"]?.ToString(),
                    IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                });
            }

            return diets;
        }

        public void Insert(DietMaster diet)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietQueries.Insert, con);

            cmd.Parameters.AddWithValue("@DietName", diet.DietName);
            cmd.Parameters.AddWithValue("@Category", diet.Category);
            cmd.Parameters.AddWithValue("@MealType", diet.MealType);
            cmd.Parameters.AddWithValue("@Calories", (object?)diet.Calories ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Protein", (object?)diet.Protein ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Carbs", (object?)diet.Carbs ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fat", (object?)diet.Fat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)diet.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", diet.IsActive);

            cmd.ExecuteNonQuery();
        }

        public DietMaster GetById(int id)
        {
            DietMaster diet = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietQueries.GetById, con);
            cmd.Parameters.AddWithValue("@DietId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                diet.DietId = Convert.ToInt32(dr["DietId"]);
                diet.DietName = dr["DietName"]?.ToString() ?? "";
                diet.Category = dr["Category"]?.ToString() ?? "";
                diet.MealType = dr["MealType"]?.ToString() ?? "";
                diet.Calories = dr["Calories"] == DBNull.Value ? null : Convert.ToDecimal(dr["Calories"]);
                diet.Protein = dr["Protein"] == DBNull.Value ? null : Convert.ToDecimal(dr["Protein"]);
                diet.Carbs = dr["Carbs"] == DBNull.Value ? null : Convert.ToDecimal(dr["Carbs"]);
                diet.Fat = dr["Fat"] == DBNull.Value ? null : Convert.ToDecimal(dr["Fat"]);
                diet.Description = dr["Description"]?.ToString();
                diet.IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]);
            }

            return diet;
        }

        public void Update(DietMaster diet)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietQueries.Update, con);

            cmd.Parameters.AddWithValue("@DietId", diet.DietId);
            cmd.Parameters.AddWithValue("@DietName", diet.DietName);
            cmd.Parameters.AddWithValue("@Category", diet.Category);
            cmd.Parameters.AddWithValue("@MealType", diet.MealType);
            cmd.Parameters.AddWithValue("@Calories", (object?)diet.Calories ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Protein", (object?)diet.Protein ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Carbs", (object?)diet.Carbs ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fat", (object?)diet.Fat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)diet.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", diet.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(DietQueries.Delete, con);
            cmd.Parameters.AddWithValue("@DietId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

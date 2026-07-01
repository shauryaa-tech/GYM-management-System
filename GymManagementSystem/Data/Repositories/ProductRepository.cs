using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class ProductRepository
    {
        private readonly DbHelper _db;

        public ProductRepository(DbHelper db)
        {
            _db = db;
        }

        public List<ProductMaster> GetAll(string? search, string? category)
        {
            List<ProductMaster> products = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = ProductQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (P.ProductName LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query += " AND P.Category=@Category";
            }

            query += " ORDER BY P.ProductId DESC";

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
                products.Add(new ProductMaster
                {
                    ProductId = Convert.ToInt32(dr["ProductId"]),
                    ProductName = dr["ProductName"]?.ToString() ?? "",
                    Category = dr["Category"]?.ToString(),
                    UnitPrice = Convert.ToDecimal(dr["UnitPrice"]),
                    CurrentStock = Convert.ToInt32(dr["CurrentStock"]),
                    ReorderLevel = Convert.ToInt32(dr["ReorderLevel"]),
                    VendorId = dr["VendorId"] == DBNull.Value ? null : Convert.ToInt32(dr["VendorId"]),
                    VendorName = dr["VendorName"]?.ToString(),
                    IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                });
            }

            return products;
        }

        public void Insert(ProductMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ProductQueries.Insert, con);

            cmd.Parameters.AddWithValue("@ProductName", model.ProductName);
            cmd.Parameters.AddWithValue("@Category", (object?)model.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UnitPrice", model.UnitPrice);
            cmd.Parameters.AddWithValue("@CurrentStock", model.CurrentStock);
            cmd.Parameters.AddWithValue("@ReorderLevel", model.ReorderLevel);
            cmd.Parameters.AddWithValue("@VendorId", (object?)model.VendorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public ProductMaster GetById(int id)
        {
            ProductMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ProductQueries.GetById, con);
            cmd.Parameters.AddWithValue("@ProductId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.ProductId = Convert.ToInt32(dr["ProductId"]);
                model.ProductName = dr["ProductName"]?.ToString() ?? "";
                model.Category = dr["Category"]?.ToString();
                model.UnitPrice = Convert.ToDecimal(dr["UnitPrice"]);
                model.CurrentStock = Convert.ToInt32(dr["CurrentStock"]);
                model.ReorderLevel = Convert.ToInt32(dr["ReorderLevel"]);
                model.VendorId = dr["VendorId"] == DBNull.Value ? null : Convert.ToInt32(dr["VendorId"]);
                model.IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Update(ProductMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ProductQueries.Update, con);

            cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
            cmd.Parameters.AddWithValue("@ProductName", model.ProductName);
            cmd.Parameters.AddWithValue("@Category", (object?)model.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UnitPrice", model.UnitPrice);
            cmd.Parameters.AddWithValue("@CurrentStock", model.CurrentStock);
            cmd.Parameters.AddWithValue("@ReorderLevel", model.ReorderLevel);
            cmd.Parameters.AddWithValue("@VendorId", (object?)model.VendorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ProductQueries.Delete, con);
            cmd.Parameters.AddWithValue("@ProductId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class StockPurchaseRepository
    {
        private readonly DbHelper _db;

        public StockPurchaseRepository(DbHelper db)
        {
            _db = db;
        }

        public List<StockPurchase> GetAll(string? search, string? productId, string? vendorId, string? fromDate, string? toDate)
        {
            List<StockPurchase> purchases = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = StockPurchaseQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (P.ProductName LIKE @Search OR SP.InvoiceNo LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(productId))
            {
                query += " AND SP.ProductId=@ProductId";
            }

            if (!string.IsNullOrWhiteSpace(vendorId))
            {
                query += " AND SP.VendorId=@VendorId";
            }

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                query += " AND SP.PurchaseDate >= @FromDate";
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                query += " AND SP.PurchaseDate <= @ToDate";
            }

            query += " ORDER BY SP.PurchaseDate DESC, SP.PurchaseId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(productId))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
            }

            if (!string.IsNullOrWhiteSpace(vendorId))
            {
                cmd.Parameters.AddWithValue("@VendorId", vendorId);
            }

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                cmd.Parameters.AddWithValue("@ToDate", toDate);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                purchases.Add(new StockPurchase
                {
                    PurchaseId = Convert.ToInt32(dr["PurchaseId"]),
                    ProductId = Convert.ToInt32(dr["ProductId"]),
                    VendorId = Convert.ToInt32(dr["VendorId"]),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    UnitPrice = Convert.ToDecimal(dr["UnitPrice"]),
                    TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
                    PurchaseDate = Convert.ToDateTime(dr["PurchaseDate"]),
                    InvoiceNo = dr["InvoiceNo"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString(),
                    ProductName = dr["ProductName"]?.ToString(),
                    Category = dr["Category"]?.ToString(),
                    VendorName = dr["VendorName"]?.ToString(),
                    CurrentStock = dr["CurrentStock"] == DBNull.Value ? null : Convert.ToInt32(dr["CurrentStock"])
                });
            }

            return purchases;
        }

        public void Insert(StockPurchase model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand(StockPurchaseQueries.Insert, con, transaction);

                cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                cmd.Parameters.AddWithValue("@UnitPrice", model.UnitPrice);
                cmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
                cmd.Parameters.AddWithValue("@PurchaseDate", model.PurchaseDate);
                cmd.Parameters.AddWithValue("@InvoiceNo", (object?)model.InvoiceNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                // Update product stock
                SqlCommand stockCmd = new SqlCommand(StockPurchaseQueries.UpdateProductStock, con, transaction);
                stockCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                stockCmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                stockCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public StockPurchase GetById(int id)
        {
            StockPurchase model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StockPurchaseQueries.GetById, con);
            cmd.Parameters.AddWithValue("@PurchaseId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.PurchaseId = Convert.ToInt32(dr["PurchaseId"]);
                model.ProductId = Convert.ToInt32(dr["ProductId"]);
                model.VendorId = Convert.ToInt32(dr["VendorId"]);
                model.Quantity = Convert.ToInt32(dr["Quantity"]);
                model.UnitPrice = Convert.ToDecimal(dr["UnitPrice"]);
                model.TotalAmount = Convert.ToDecimal(dr["TotalAmount"]);
                model.PurchaseDate = Convert.ToDateTime(dr["PurchaseDate"]);
                model.InvoiceNo = dr["InvoiceNo"]?.ToString();
                model.Remarks = dr["Remarks"]?.ToString();
                model.ProductName = dr["ProductName"]?.ToString();
                model.Category = dr["Category"]?.ToString();
                model.VendorName = dr["VendorName"]?.ToString();
                model.CurrentStock = dr["CurrentStock"] == DBNull.Value ? null : Convert.ToInt32(dr["CurrentStock"]);
            }

            return model;
        }

        public void Update(StockPurchase model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                // Get old quantity to adjust stock
                var oldPurchase = GetById(model.PurchaseId);
                int oldQuantity = oldPurchase.Quantity;

                SqlCommand cmd = new SqlCommand(StockPurchaseQueries.Update, con, transaction);

                cmd.Parameters.AddWithValue("@PurchaseId", model.PurchaseId);
                cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                cmd.Parameters.AddWithValue("@UnitPrice", model.UnitPrice);
                cmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
                cmd.Parameters.AddWithValue("@PurchaseDate", model.PurchaseDate);
                cmd.Parameters.AddWithValue("@InvoiceNo", (object?)model.InvoiceNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                // Adjust product stock (difference)
                int diff = model.Quantity - oldQuantity;
                if (diff != 0)
                {
                    SqlCommand stockCmd = new SqlCommand(StockPurchaseQueries.UpdateProductStock, con, transaction);
                    stockCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                    stockCmd.Parameters.AddWithValue("@Quantity", diff);
                    stockCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                // Get purchase to adjust stock
                var purchase = GetById(id);

                SqlCommand cmd = new SqlCommand(StockPurchaseQueries.Delete, con, transaction);
                cmd.Parameters.AddWithValue("@PurchaseId", id);
                cmd.ExecuteNonQuery();

                // Reverse stock
                SqlCommand stockCmd = new SqlCommand(StockPurchaseQueries.UpdateProductStock, con, transaction);
                stockCmd.Parameters.AddWithValue("@ProductId", purchase.ProductId);
                stockCmd.Parameters.AddWithValue("@Quantity", -purchase.Quantity);
                stockCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<ProductMaster> GetActiveProducts()
        {
            List<ProductMaster> products = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StockPurchaseQueries.GetActiveProducts, con);

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
                    VendorId = dr["VendorId"] == DBNull.Value ? null : Convert.ToInt32(dr["VendorId"])
                });
            }

            return products;
        }

        public List<VendorMaster> GetActiveVendors()
        {
            List<VendorMaster> vendors = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StockPurchaseQueries.GetActiveVendors, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                vendors.Add(new VendorMaster
                {
                    VendorId = Convert.ToInt32(dr["VendorId"]),
                    VendorName = dr["VendorName"]?.ToString() ?? ""
                });
            }

            return vendors;
        }
    }
}
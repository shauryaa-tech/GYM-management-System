using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class StockIssueRepository
    {
        private readonly DbHelper _db;

        public StockIssueRepository(DbHelper db)
        {
            _db = db;
        }

        public List<StockIssue> GetAll(string? search, string? productId, string? memberId, string? fromDate, string? toDate)
        {
            List<StockIssue> issues = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = StockIssueQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (P.ProductName LIKE @Search OR SI.IssuedTo LIKE @Search OR M.MemberName LIKE @Search OR M.MemberCode LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(productId))
            {
                query += " AND SI.ProductId=@ProductId";
            }

            if (!string.IsNullOrWhiteSpace(memberId))
            {
                query += " AND SI.MemberId=@MemberId";
            }

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                query += " AND SI.IssueDate >= @FromDate";
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                query += " AND SI.IssueDate <= @ToDate";
            }

            query += " ORDER BY SI.IssueDate DESC, SI.IssueId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(productId))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
            }

            if (!string.IsNullOrWhiteSpace(memberId))
            {
                cmd.Parameters.AddWithValue("@MemberId", memberId);
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
                issues.Add(MapStockIssue(dr));
            }

            return issues;
        }

        public void Insert(StockIssue model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand(StockIssueQueries.Insert, con, transaction);

                cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                cmd.Parameters.AddWithValue("@MemberId", (object?)model.MemberId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                cmd.Parameters.AddWithValue("@IssueDate", model.IssueDate);
                cmd.Parameters.AddWithValue("@IssuedTo", model.IssuedTo);
                cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                SqlCommand stockCmd = new SqlCommand(StockIssueQueries.UpdateProductStock, con, transaction);
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

        public StockIssue GetById(int id)
        {
            StockIssue model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StockIssueQueries.GetById, con);
            cmd.Parameters.AddWithValue("@IssueId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model = MapStockIssue(dr);
            }

            return model;
        }

        public void Update(StockIssue model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                var oldIssue = GetById(model.IssueId);
                int oldQuantity = oldIssue.Quantity;
                int oldProductId = oldIssue.ProductId;

                SqlCommand cmd = new SqlCommand(StockIssueQueries.Update, con, transaction);

                cmd.Parameters.AddWithValue("@IssueId", model.IssueId);
                cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                cmd.Parameters.AddWithValue("@MemberId", (object?)model.MemberId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                cmd.Parameters.AddWithValue("@IssueDate", model.IssueDate);
                cmd.Parameters.AddWithValue("@IssuedTo", model.IssuedTo);
                cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                if (oldProductId == model.ProductId)
                {
                    int diff = model.Quantity - oldQuantity;
                    if (diff != 0)
                    {
                        SqlCommand stockCmd = new SqlCommand(StockIssueQueries.UpdateProductStock, con, transaction);
                        stockCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                        stockCmd.Parameters.AddWithValue("@Quantity", diff);
                        stockCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    SqlCommand restoreCmd = new SqlCommand(StockIssueQueries.UpdateProductStock, con, transaction);
                    restoreCmd.Parameters.AddWithValue("@ProductId", oldProductId);
                    restoreCmd.Parameters.AddWithValue("@Quantity", -oldQuantity);
                    restoreCmd.ExecuteNonQuery();

                    SqlCommand deductCmd = new SqlCommand(StockIssueQueries.UpdateProductStock, con, transaction);
                    deductCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                    deductCmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                    deductCmd.ExecuteNonQuery();
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
                var issue = GetById(id);

                SqlCommand cmd = new SqlCommand(StockIssueQueries.Delete, con, transaction);
                cmd.Parameters.AddWithValue("@IssueId", id);
                cmd.ExecuteNonQuery();

                SqlCommand stockCmd = new SqlCommand(StockIssueQueries.UpdateProductStock, con, transaction);
                stockCmd.Parameters.AddWithValue("@ProductId", issue.ProductId);
                stockCmd.Parameters.AddWithValue("@Quantity", -issue.Quantity);
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

            SqlCommand cmd = new SqlCommand(StockIssueQueries.GetActiveProducts, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                products.Add(new ProductMaster
                {
                    ProductId = Convert.ToInt32(dr["ProductId"]),
                    ProductName = dr["ProductName"]?.ToString() ?? "",
                    Category = dr["Category"]?.ToString(),
                    UnitPrice = Convert.ToDecimal(dr["UnitPrice"]),
                    CurrentStock = Convert.ToInt32(dr["CurrentStock"])
                });
            }

            return products;
        }

        public List<MemberMaster> GetActiveMembers()
        {
            List<MemberMaster> members = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StockIssueQueries.GetActiveMembers, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                members.Add(new MemberMaster
                {
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    MemberCode = dr["MemberCode"]?.ToString() ?? "",
                    MemberName = dr["MemberName"]?.ToString() ?? ""
                });
            }

            return members;
        }

        private static StockIssue MapStockIssue(SqlDataReader dr)
        {
            return new StockIssue
            {
                IssueId = Convert.ToInt32(dr["IssueId"]),
                ProductId = Convert.ToInt32(dr["ProductId"]),
                MemberId = dr["MemberId"] == DBNull.Value ? null : Convert.ToInt32(dr["MemberId"]),
                Quantity = Convert.ToInt32(dr["Quantity"]),
                IssueDate = Convert.ToDateTime(dr["IssueDate"]),
                IssuedTo = dr["IssuedTo"]?.ToString() ?? "",
                Amount = dr["Amount"] == DBNull.Value ? null : Convert.ToDecimal(dr["Amount"]),
                PaymentMode = dr["PaymentMode"]?.ToString(),
                Remarks = dr["Remarks"]?.ToString(),
                ProductName = dr["ProductName"]?.ToString(),
                Category = dr["Category"]?.ToString(),
                MemberName = dr["MemberName"]?.ToString(),
                MemberCode = dr["MemberCode"]?.ToString(),
                CurrentStock = dr["CurrentStock"] == DBNull.Value ? null : Convert.ToInt32(dr["CurrentStock"])
            };
        }
    }
}

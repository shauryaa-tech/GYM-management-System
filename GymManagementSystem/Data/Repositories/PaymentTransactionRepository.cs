using GymManagement.Data.Queries;
using GymManagement.Data.Repositories.Interfaces;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly DbHelper _db;

        public PaymentTransactionRepository(DbHelper db)
        {
            _db = db;
        }

        public Task<PaymentTransaction?> GetByOrderIdAsync(string orderId)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentTransactionQueries.GetByOrderId, con);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            using SqlDataReader dr = cmd.ExecuteReader();
            return Task.FromResult(dr.Read() ? MapTransaction(dr) : null);
        }

        public Task AddAsync(PaymentTransaction transaction)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentTransactionQueries.Insert + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
            BindTransaction(cmd, transaction, includeId: false);
            transaction.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return Task.CompletedTask;
        }

        public Task UpdateAsync(PaymentTransaction transaction)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentTransactionQueries.Update, con);
            BindTransaction(cmd, transaction, includeId: true);
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        private static PaymentTransaction MapTransaction(SqlDataReader dr) => new()
        {
            Id = Convert.ToInt32(dr["Id"]),
            MemberId = dr["MemberId"] == DBNull.Value ? null : Convert.ToInt32(dr["MemberId"]),
            OrderId = dr["OrderId"]?.ToString() ?? "",
            TransactionId = dr["TransactionId"]?.ToString(),
            Gateway = dr["Gateway"]?.ToString() ?? "",
            Amount = Convert.ToDecimal(dr["Amount"]),
            Currency = dr["Currency"]?.ToString() ?? "INR",
            PaymentFor = dr["PaymentFor"]?.ToString(),
            Status = dr["Status"]?.ToString() ?? "Pending",
            ResponseCode = dr["ResponseCode"]?.ToString(),
            ResponseMessage = dr["ResponseMessage"]?.ToString(),
            GatewayResponse = dr["GatewayResponse"]?.ToString(),
            PaidOn = dr["PaidOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["PaidOn"]),
            CreatedDate = Convert.ToDateTime(dr["CreatedDate"])
        };

        private static void BindTransaction(SqlCommand cmd, PaymentTransaction t, bool includeId)
        {
            if (includeId)
                cmd.Parameters.AddWithValue("@Id", t.Id);

            cmd.Parameters.AddWithValue("@MemberId", (object?)t.MemberId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrderId", t.OrderId);
            cmd.Parameters.AddWithValue("@TransactionId", (object?)t.TransactionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gateway", t.Gateway);
            cmd.Parameters.AddWithValue("@Amount", t.Amount);
            cmd.Parameters.AddWithValue("@Currency", t.Currency);
            cmd.Parameters.AddWithValue("@PaymentFor", (object?)t.PaymentFor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", t.Status);
            cmd.Parameters.AddWithValue("@ResponseCode", (object?)t.ResponseCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ResponseMessage", (object?)t.ResponseMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GatewayResponse", (object?)t.GatewayResponse ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaidOn", (object?)t.PaidOn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDate", t.CreatedDate);
        }
    }
}

using GymManagement.Data;
using GymManagement.Data.Queries;
using GymManagement.Models;
using GymManagementSystem.Data.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GymManagement.Repositories
{
    public class MembershipTransactionRepository
    {
        private readonly DbHelper _db;

        public MembershipTransactionRepository(DbHelper db)
        {
            _db = db;
        }

        public List<MembershipTransaction> GetAll()
        {
            List<MembershipTransaction> list = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembershipTransactionQueries.GetAll,
                    con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new MembershipTransaction
                {
                    TransactionId = Convert.ToInt32(dr["TransactionId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    PlanId = Convert.ToInt32(dr["PlanId"]),
                    MemberName = dr["MemberName"]?.ToString(),
                    PlanName = dr["PlanName"]?.ToString(),
                    Amount = Convert.ToDecimal(dr["Amount"]),
                    StartDate = Convert.ToDateTime(dr["StartDate"]),
                    EndDate = Convert.ToDateTime(dr["EndDate"]),
                    PaymentStatus = dr["PaymentStatus"]?.ToString(),
                    MembershipStatus = dr["MembershipStatus"]?.ToString()
                });
            }

            return list;
        }

        public void Insert(MembershipTransaction model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(
                MembershipTransactionQueries.Insert,
                con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@PlanId", model.PlanId);
            cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@PaymentStatus", model.PaymentStatus);
            cmd.Parameters.AddWithValue("@MembershipStatus", model.MembershipStatus);
            cmd.Parameters.AddWithValue("@Remarks",
                (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembershipTransactionQueries.Delete,
                    con);

            cmd.Parameters.AddWithValue(
                "@TransactionId",
                id);

            cmd.ExecuteNonQuery();
        }

        public MembershipTransaction? GetById(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(MembershipTransactionQueries.GetById, con);
            cmd.Parameters.AddWithValue("@TransactionId", id);

            using SqlDataReader dr = cmd.ExecuteReader();
            if (!dr.Read())
                return null;

            return new MembershipTransaction
            {
                TransactionId = Convert.ToInt32(dr["TransactionId"]),
                MemberId = Convert.ToInt32(dr["MemberId"]),
                PlanId = Convert.ToInt32(dr["PlanId"]),
                MemberName = dr["MemberName"]?.ToString(),
                PlanName = dr["PlanName"]?.ToString(),
                Amount = Convert.ToDecimal(dr["Amount"]),
                StartDate = Convert.ToDateTime(dr["StartDate"]),
                EndDate = Convert.ToDateTime(dr["EndDate"]),
                PaymentStatus = dr["PaymentStatus"]?.ToString(),
                MembershipStatus = dr["MembershipStatus"]?.ToString(),
                Remarks = dr["Remarks"]?.ToString()
            };
        }

        public bool UpdatePaymentStatus(int transactionId, string paymentStatus, string? remarks = null)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(MembershipTransactionQueries.UpdatePaymentStatus, con);
            cmd.Parameters.AddWithValue("@TransactionId", transactionId);
            cmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
            cmd.Parameters.AddWithValue("@Remarks", (object?)remarks ?? DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
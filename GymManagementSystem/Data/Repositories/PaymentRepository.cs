using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class PaymentRepository
    {
        private readonly DbHelper _db;

        public PaymentRepository(DbHelper db)
        {
            _db = db;
        }

        public List<Payment> GetAll(DateTime? fromDate, DateTime? toDate, string? mode)
        {
            List<Payment> payments = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = PaymentQueries.GetAll;

            if (fromDate.HasValue)
            {
                query += " AND P.PaymentDate >= @FromDate";
            }
            if (toDate.HasValue)
            {
                query += " AND P.PaymentDate <= @ToDate";
            }
            if (!string.IsNullOrWhiteSpace(mode))
            {
                query += " AND P.PaymentMode = @Mode";
            }

            query += " ORDER BY P.PaymentId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (fromDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@ToDate", toDate.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(mode))
            {
                cmd.Parameters.AddWithValue("@Mode", mode);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                payments.Add(new Payment
                {
                    PaymentId = Convert.ToInt32(dr["PaymentId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    PaymentDate = Convert.ToDateTime(dr["PaymentDate"]),
                    Amount = Convert.ToDecimal(dr["Amount"]),
                    PaymentMode = dr["PaymentMode"]?.ToString(),
                    ReferenceNo = dr["ReferenceNo"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString()
                });
            }

            return payments;
        }

        public void Insert(Payment model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(PaymentQueries.Insert, con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@PaymentDate", model.PaymentDate);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReferenceNo", (object?)model.ReferenceNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(PaymentQueries.Delete, con);
            cmd.Parameters.AddWithValue("@PaymentId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class ExpenseRepository
    {
        private readonly DbHelper _db;

        public ExpenseRepository(DbHelper db)
        {
            _db = db;
        }

        public List<Expense> GetAll(string? search, string? expenseHeadId, string? paymentMode, string? fromDate, string? toDate)
        {
            List<Expense> expenses = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = ExpenseQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (EH.HeadName LIKE @Search OR E.Description LIKE @Search OR E.PaidTo LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(expenseHeadId))
            {
                query += " AND E.ExpenseHeadId=@ExpenseHeadId";
            }

            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                query += " AND E.PaymentMode=@PaymentMode";
            }

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                query += " AND E.ExpenseDate >= @FromDate";
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                query += " AND E.ExpenseDate <= @ToDate";
            }

            query += " ORDER BY E.ExpenseDate DESC, E.ExpenseId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(expenseHeadId))
            {
                cmd.Parameters.AddWithValue("@ExpenseHeadId", expenseHeadId);
            }

            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                cmd.Parameters.AddWithValue("@PaymentMode", paymentMode);
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
                expenses.Add(MapExpense(dr));
            }

            return expenses;
        }

        public void Insert(Expense model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseQueries.Insert, con);

            cmd.Parameters.AddWithValue("@ExpenseHeadId", model.ExpenseHeadId);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaidTo", (object?)model.PaidTo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public Expense GetById(int id)
        {
            Expense model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseQueries.GetById, con);
            cmd.Parameters.AddWithValue("@ExpenseId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model = MapExpense(dr);
            }

            return model;
        }

        public void Update(Expense model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseQueries.Update, con);

            cmd.Parameters.AddWithValue("@ExpenseId", model.ExpenseId);
            cmd.Parameters.AddWithValue("@ExpenseHeadId", model.ExpenseHeadId);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaidTo", (object?)model.PaidTo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseQueries.Delete, con);
            cmd.Parameters.AddWithValue("@ExpenseId", id);

            cmd.ExecuteNonQuery();
        }

        public List<ExpenseHeadMaster> GetActiveExpenseHeads()
        {
            List<ExpenseHeadMaster> heads = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ExpenseQueries.GetActiveExpenseHeads, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                heads.Add(new ExpenseHeadMaster
                {
                    ExpenseHeadId = Convert.ToInt32(dr["ExpenseHeadId"]),
                    HeadName = dr["HeadName"]?.ToString() ?? ""
                });
            }

            return heads;
        }

        private static Expense MapExpense(SqlDataReader dr)
        {
            return new Expense
            {
                ExpenseId = Convert.ToInt32(dr["ExpenseId"]),
                ExpenseHeadId = Convert.ToInt32(dr["ExpenseHeadId"]),
                Amount = Convert.ToDecimal(dr["Amount"]),
                ExpenseDate = Convert.ToDateTime(dr["ExpenseDate"]),
                Description = dr["Description"]?.ToString(),
                PaymentMode = dr["PaymentMode"]?.ToString(),
                PaidTo = dr["PaidTo"]?.ToString(),
                Remarks = dr["Remarks"]?.ToString(),
                HeadName = dr["HeadName"]?.ToString()
            };
        }
    }
}

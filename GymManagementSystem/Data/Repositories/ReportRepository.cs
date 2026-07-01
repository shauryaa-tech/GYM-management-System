using GymManagement.Data.Queries;
using GymManagement.ViewModels;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class ReportRepository
    {
        private readonly DbHelper _db;

        public ReportRepository(DbHelper db)
        {
            _db = db;
        }

        public AttendanceReportViewModel GetAttendanceReport(DateTime fromDate, DateTime toDate)
        {
            var model = new AttendanceReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date
            };

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand statsCmd = new SqlCommand(ReportQueries.AttendanceStats, con);
            statsCmd.Parameters.AddWithValue("@FromDate", model.FromDate);
            statsCmd.Parameters.AddWithValue("@ToDate", model.ToDate);

            using (SqlDataReader statsDr = statsCmd.ExecuteReader())
            {
                if (statsDr.Read())
                {
                    model.TotalRecords = Convert.ToInt32(statsDr["TotalRecords"]);
                    model.UniqueMembers = Convert.ToInt32(statsDr["UniqueMembers"]);
                }
            }

            SqlCommand cmd = new SqlCommand(ReportQueries.AttendanceReport, con);
            cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
            cmd.Parameters.AddWithValue("@ToDate", model.ToDate);

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                model.Rows.Add(new AttendanceReportRow
                {
                    AttendanceId = Convert.ToInt32(dr["AttendanceId"]),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    AttendanceDate = Convert.ToDateTime(dr["AttendanceDate"]),
                    CheckInTime = dr["CheckInTime"] == DBNull.Value ? null : (TimeSpan?)dr["CheckInTime"],
                    CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? null : (TimeSpan?)dr["CheckOutTime"],
                    Remarks = dr["Remarks"]?.ToString()
                });
            }

            return model;
        }

        public ExpiryReportViewModel GetExpiryReport(int daysAhead)
        {
            var model = new ExpiryReportViewModel
            {
                DaysAhead = daysAhead
            };

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand statsCmd = new SqlCommand(ReportQueries.ExpiryStats, con);
            statsCmd.Parameters.AddWithValue("@DaysAhead", daysAhead);

            using (SqlDataReader statsDr = statsCmd.ExecuteReader())
            {
                if (statsDr.Read())
                {
                    model.ExpiredCount = statsDr["ExpiredCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(statsDr["ExpiredCount"]);
                    model.ExpiringSoonCount = statsDr["ExpiringSoonCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(statsDr["ExpiringSoonCount"]);
                }
            }

            SqlCommand cmd = new SqlCommand(ReportQueries.ExpiryReport, con);
            cmd.Parameters.AddWithValue("@DaysAhead", daysAhead);

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                model.Rows.Add(new ExpiryReportRow
                {
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MobileNo = dr["MobileNo"]?.ToString(),
                    PlanName = dr["PlanName"]?.ToString(),
                    PlanEndDate = Convert.ToDateTime(dr["PlanEndDate"]),
                    DaysLeft = Convert.ToInt32(dr["DaysLeft"]),
                    Status = dr["Status"]?.ToString()
                });
            }

            return model;
        }

        public CollectionReportViewModel GetCollectionsReport(
            DateTime fromDate,
            DateTime toDate,
            string? paymentMode)
        {
            var model = new CollectionReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                PaymentMode = paymentMode
            };

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string statsQuery = ReportQueries.CollectionsStats;
            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                statsQuery += " AND C.PaymentMode = @PaymentMode";
            }

            SqlCommand statsCmd = new SqlCommand(statsQuery, con);
            statsCmd.Parameters.AddWithValue("@FromDate", model.FromDate);
            statsCmd.Parameters.AddWithValue("@ToDate", model.ToDate);
            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                statsCmd.Parameters.AddWithValue("@PaymentMode", paymentMode);
            }

            using (SqlDataReader statsDr = statsCmd.ExecuteReader())
            {
                if (statsDr.Read())
                {
                    model.TotalRecords = Convert.ToInt32(statsDr["TotalRecords"]);
                    model.TotalAmount = Convert.ToDecimal(statsDr["TotalAmount"]);
                    model.CashTotal = Convert.ToDecimal(statsDr["CashTotal"]);
                    model.UpiTotal = Convert.ToDecimal(statsDr["UpiTotal"]);
                    model.CardTotal = Convert.ToDecimal(statsDr["CardTotal"]);
                }
            }

            string query = ReportQueries.CollectionsReport;
            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                query += " AND C.PaymentMode = @PaymentMode";
            }
            query += " ORDER BY C.PaymentDate DESC, C.PaymentId DESC";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@FromDate", model.FromDate);
            cmd.Parameters.AddWithValue("@ToDate", model.ToDate);
            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                cmd.Parameters.AddWithValue("@PaymentMode", paymentMode);
            }

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                model.Rows.Add(new CollectionReportRow
                {
                    PaymentId = Convert.ToInt32(dr["PaymentId"]),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    PaymentDate = Convert.ToDateTime(dr["PaymentDate"]),
                    Amount = Convert.ToDecimal(dr["Amount"]),
                    PaymentMode = dr["PaymentMode"]?.ToString(),
                    ReferenceNo = dr["ReferenceNo"]?.ToString()
                });
            }

            return model;
        }

        public OutstandingReportViewModel GetOutstandingReport(string? statusFilter)
        {
            var model = new OutstandingReportViewModel
            {
                StatusFilter = statusFilter
            };

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string statsQuery = ReportQueries.OutstandingStats;
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                statsQuery += " AND MT.PaymentStatus = @StatusFilter";
            }

            SqlCommand statsCmd = new SqlCommand(statsQuery, con);
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                statsCmd.Parameters.AddWithValue("@StatusFilter", statusFilter);
            }

            using (SqlDataReader statsDr = statsCmd.ExecuteReader())
            {
                if (statsDr.Read())
                {
                    model.TotalOutstanding = Convert.ToDecimal(statsDr["TotalOutstanding"]);
                    model.PendingCount = statsDr["PendingCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(statsDr["PendingCount"]);
                    model.PartialCount = statsDr["PartialCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(statsDr["PartialCount"]);
                }
            }

            string query = ReportQueries.OutstandingReport;
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query += " AND MT.PaymentStatus = @StatusFilter";
            }
            query += " ORDER BY MT.EndDate ASC";

            SqlCommand cmd = new SqlCommand(query, con);
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                cmd.Parameters.AddWithValue("@StatusFilter", statusFilter);
            }

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                model.Rows.Add(new OutstandingReportRow
                {
                    TransactionId = Convert.ToInt32(dr["TransactionId"]),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MobileNo = dr["MobileNo"]?.ToString(),
                    PlanName = dr["PlanName"]?.ToString(),
                    Amount = Convert.ToDecimal(dr["Amount"]),
                    PaymentStatus = dr["PaymentStatus"]?.ToString(),
                    EndDate = Convert.ToDateTime(dr["EndDate"])
                });
            }

            return model;
        }

        public ProfitLossReportViewModel GetProfitLossReport(DateTime fromDate, DateTime toDate)
        {
            var model = new ProfitLossReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date
            };

            using SqlConnection con = _db.GetConnection();
            con.Open();

            model.TotalIncome = ExecuteDecimal(con, ReportQueries.ProfitLossIncome, model.FromDate, model.ToDate);
            model.TotalExpenses = ExecuteDecimal(con, ReportQueries.ProfitLossExpenses, model.FromDate, model.ToDate);
            model.TotalSalaries = ExecuteDecimal(con, ReportQueries.ProfitLossSalaries, model.FromDate, model.ToDate);
            model.NetProfit = model.TotalIncome - model.TotalExpenses - model.TotalSalaries;

            SqlCommand expenseCmd = new SqlCommand(ReportQueries.ProfitLossExpenseBreakdown, con);
            expenseCmd.Parameters.AddWithValue("@FromDate", model.FromDate);
            expenseCmd.Parameters.AddWithValue("@ToDate", model.ToDate);

            using (SqlDataReader dr = expenseCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    model.ExpenseBreakdown.Add(new ProfitLossBreakdownRow
                    {
                        Label = dr["Label"]?.ToString() ?? "Other",
                        Amount = Convert.ToDecimal(dr["Amount"])
                    });
                }
            }

            SqlCommand incomeCmd = new SqlCommand(ReportQueries.ProfitLossIncomeByMode, con);
            incomeCmd.Parameters.AddWithValue("@FromDate", model.FromDate);
            incomeCmd.Parameters.AddWithValue("@ToDate", model.ToDate);

            using (SqlDataReader dr = incomeCmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    model.IncomeByMode.Add(new ProfitLossBreakdownRow
                    {
                        Label = dr["Label"]?.ToString() ?? "Other",
                        Amount = Convert.ToDecimal(dr["Amount"])
                    });
                }
            }

            return model;
        }

        private static decimal ExecuteDecimal(SqlConnection con, string sql, DateTime fromDate, DateTime toDate)
        {
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate);
            cmd.Parameters.AddWithValue("@ToDate", toDate);
            object? result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }
    }
}

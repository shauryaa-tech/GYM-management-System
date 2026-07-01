using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class SalaryProcessingRepository
    {
        private readonly DbHelper _db;

        public SalaryProcessingRepository(DbHelper db)
        {
            _db = db;
        }

        public List<SalaryProcessing> GetAll(string? search, string? staffId, string? month, string? year, string? paymentStatus = null)
        {
            List<SalaryProcessing> salaries = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = SalaryProcessingQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (S.StaffName LIKE @Search OR SP.Remarks LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(staffId))
            {
                query += " AND SP.StaffId=@StaffId";
            }

            if (!string.IsNullOrWhiteSpace(month))
            {
                query += " AND SP.Month=@Month";
            }

            if (!string.IsNullOrWhiteSpace(year))
            {
                query += " AND SP.Year=@Year";
            }

            if (string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            {
                query += " AND SP.PaidDate IS NOT NULL AND (SP.PaymentMode IS NULL OR SP.PaymentMode <> 'Pending')";
            }
            else if (string.Equals(paymentStatus, "pending", StringComparison.OrdinalIgnoreCase))
            {
                query += " AND (SP.PaidDate IS NULL OR SP.PaymentMode = 'Pending')";
            }

            query += " ORDER BY SP.Year DESC, SP.Month DESC, SP.SalaryId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(staffId))
            {
                cmd.Parameters.AddWithValue("@StaffId", staffId);
            }

            if (!string.IsNullOrWhiteSpace(month))
            {
                cmd.Parameters.AddWithValue("@Month", month);
            }

            if (!string.IsNullOrWhiteSpace(year))
            {
                cmd.Parameters.AddWithValue("@Year", year);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                salaries.Add(MapSalaryProcessing(dr));
            }

            return salaries;
        }

        public void Insert(SalaryProcessing model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.Insert, con);

            cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
            cmd.Parameters.AddWithValue("@Month", model.Month);
            cmd.Parameters.AddWithValue("@Year", model.Year);
            cmd.Parameters.AddWithValue("@BasicSalary", model.BasicSalary);
            cmd.Parameters.AddWithValue("@Deductions", model.Deductions);
            cmd.Parameters.AddWithValue("@NetSalary", model.NetSalary);
            cmd.Parameters.AddWithValue("@PaidDate", (object?)model.PaidDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public SalaryProcessing GetById(int id)
        {
            SalaryProcessing model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.GetById, con);
            cmd.Parameters.AddWithValue("@SalaryId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model = MapSalaryProcessing(dr);
            }

            return model;
        }

        public void Update(SalaryProcessing model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.Update, con);

            cmd.Parameters.AddWithValue("@SalaryId", model.SalaryId);
            cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
            cmd.Parameters.AddWithValue("@Month", model.Month);
            cmd.Parameters.AddWithValue("@Year", model.Year);
            cmd.Parameters.AddWithValue("@BasicSalary", model.BasicSalary);
            cmd.Parameters.AddWithValue("@Deductions", model.Deductions);
            cmd.Parameters.AddWithValue("@NetSalary", model.NetSalary);
            cmd.Parameters.AddWithValue("@PaidDate", (object?)model.PaidDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PresentDays", (object?)model.PresentDays ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AbsentDays", (object?)model.AbsentDays ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveDays", (object?)model.LeaveDays ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HalfDays", (object?)model.HalfDays ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.Delete, con);
            cmd.Parameters.AddWithValue("@SalaryId", id);

            cmd.ExecuteNonQuery();
        }

        public List<StaffMaster> GetActiveStaff()
        {
            List<StaffMaster> staffList = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.GetActiveStaff, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                staffList.Add(new StaffMaster
                {
                    StaffId = Convert.ToInt32(dr["StaffId"]),
                    StaffName = dr["StaffName"]?.ToString() ?? "",
                    Designation = dr["Designation"]?.ToString() ?? "",
                    Salary = Convert.ToDecimal(dr["Salary"])
                });
            }

            return staffList;
        }

        public bool ExistsForPeriod(int staffId, int month, int year)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.ExistsForPeriod, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@Month", month);
            cmd.Parameters.AddWithValue("@Year", year);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public SalaryProcessing? GetByStaffPeriod(int staffId, int month, int year)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.GetByStaffPeriod, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@Month", month);
            cmd.Parameters.AddWithValue("@Year", year);
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read() ? MapSalaryProcessing(dr) : null;
        }

        public void InsertWithBreakdown(SalaryProcessing model)
        {
            InsertWithBreakdownAndGetId(model);
        }

        public int InsertWithBreakdownAndGetId(SalaryProcessing model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            try
            {
                using SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.InsertWithBreakdown, con);
                AddSalaryParams(cmd, model, includeBreakdown: true);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (SqlException)
            {
                using SqlCommand cmd = new SqlCommand(
                    SalaryProcessingQueries.Insert + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
                AddSalaryParams(cmd, model, includeBreakdown: false);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void MarkPaid(int salaryId, DateTime paidDate, string paymentMode, string? remarks)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(SalaryProcessingQueries.MarkPaid, con);
            cmd.Parameters.AddWithValue("@SalaryId", salaryId);
            cmd.Parameters.AddWithValue("@PaidDate", paidDate);
            cmd.Parameters.AddWithValue("@PaymentMode", paymentMode);
            cmd.Parameters.AddWithValue("@Remarks", (object?)remarks ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static void AddSalaryParams(SqlCommand cmd, SalaryProcessing model, bool includeBreakdown)
        {
            cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
            cmd.Parameters.AddWithValue("@Month", model.Month);
            cmd.Parameters.AddWithValue("@Year", model.Year);
            cmd.Parameters.AddWithValue("@BasicSalary", model.BasicSalary);
            cmd.Parameters.AddWithValue("@Deductions", model.Deductions);
            cmd.Parameters.AddWithValue("@NetSalary", model.NetSalary);
            cmd.Parameters.AddWithValue("@PaidDate", (object?)model.PaidDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            if (includeBreakdown)
            {
                cmd.Parameters.AddWithValue("@PresentDays", (object?)model.PresentDays ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AbsentDays", (object?)model.AbsentDays ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LeaveDays", (object?)model.LeaveDays ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HalfDays", (object?)model.HalfDays ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SalaryRuleId", (object?)model.SalaryRuleId ?? DBNull.Value);
            }
        }

        private static SalaryProcessing MapSalaryProcessing(SqlDataReader dr)
        {
            var model = new SalaryProcessing
            {
                SalaryId = Convert.ToInt32(dr["SalaryId"]),
                StaffId = Convert.ToInt32(dr["StaffId"]),
                Month = Convert.ToInt32(dr["Month"]),
                Year = Convert.ToInt32(dr["Year"]),
                BasicSalary = Convert.ToDecimal(dr["BasicSalary"]),
                Deductions = Convert.ToDecimal(dr["Deductions"]),
                NetSalary = Convert.ToDecimal(dr["NetSalary"]),
                PaidDate = dr["PaidDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["PaidDate"]),
                PaymentMode = dr["PaymentMode"]?.ToString(),
                Remarks = dr["Remarks"]?.ToString(),
                StaffName = dr["StaffName"]?.ToString(),
                StaffCode = HasColumn(dr, "StaffCode") ? dr["StaffCode"]?.ToString() : null,
                Designation = dr["Designation"]?.ToString()
            };

            if (HasColumn(dr, "PresentDays"))
                model.PresentDays = dr["PresentDays"] == DBNull.Value ? null : Convert.ToInt32(dr["PresentDays"]);
            if (HasColumn(dr, "AbsentDays"))
                model.AbsentDays = dr["AbsentDays"] == DBNull.Value ? null : Convert.ToInt32(dr["AbsentDays"]);
            if (HasColumn(dr, "LeaveDays"))
                model.LeaveDays = dr["LeaveDays"] == DBNull.Value ? null : Convert.ToInt32(dr["LeaveDays"]);
            if (HasColumn(dr, "HalfDays"))
                model.HalfDays = dr["HalfDays"] == DBNull.Value ? null : Convert.ToInt32(dr["HalfDays"]);
            if (HasColumn(dr, "SalaryRuleId"))
                model.SalaryRuleId = dr["SalaryRuleId"] == DBNull.Value ? null : Convert.ToInt32(dr["SalaryRuleId"]);

            return model;
        }

        private static bool HasColumn(SqlDataReader dr, string name)
        {
            for (int i = 0; i < dr.FieldCount; i++)
                if (dr.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}

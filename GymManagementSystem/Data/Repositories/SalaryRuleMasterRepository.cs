using GymManagement.Data.Queries;
using GymManagement.Helpers;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class SalaryRuleMasterRepository
    {
        private readonly DbHelper _db;

        public SalaryRuleMasterRepository(DbHelper db) => _db = db;

        public List<SalaryRuleMaster> GetAll(string? search = null)
        {
            var list = new List<SalaryRuleMaster>();
            using SqlConnection con = _db.GetConnection();
            con.Open();

            var query = SalaryRuleMasterQueries.GetAll;
            if (!string.IsNullOrWhiteSpace(search))
                query += " AND (RuleName LIKE @Search OR Description LIKE @Search)";

            query += " ORDER BY IsDefault DESC, RuleName";

            using SqlCommand cmd = new SqlCommand(query, con);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(Map(dr));
            return list;
        }

        public List<SalaryRuleMaster> GetActive() => GetAll().Where(r => r.IsActive).ToList();

        public SalaryRuleMaster? GetById(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(SalaryRuleMasterQueries.GetById, con);
            cmd.Parameters.AddWithValue("@RuleId", id);
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read() ? Map(dr) : null;
        }

        public SalaryRuleMaster? GetDefault()
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();
                using SqlCommand cmd = new SqlCommand(SalaryRuleMasterQueries.GetDefault, con);
                using SqlDataReader dr = cmd.ExecuteReader();
                return dr.Read() ? Map(dr) : null;
            }
            catch
            {
                return null;
            }
        }

        public SalaryRuleMaster? ResolveRule(int? ruleId)
        {
            if (ruleId.HasValue && ruleId > 0)
            {
                var rule = GetById(ruleId.Value);
                if (rule != null && rule.IsActive) return rule;
            }
            return GetDefault() ?? GetActive().FirstOrDefault();
        }

        public void Insert(SalaryRuleMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            if (model.IsDefault)
            {
                using SqlCommand clear = new SqlCommand(SalaryRuleMasterQueries.ClearDefault, con);
                clear.ExecuteNonQuery();
            }

            using SqlCommand cmd = new SqlCommand(SalaryRuleMasterQueries.Insert, con);
            BindParams(cmd, model);
            cmd.ExecuteNonQuery();
        }

        public void Update(SalaryRuleMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            if (model.IsDefault)
            {
                using SqlCommand clear = new SqlCommand(SalaryRuleMasterQueries.ClearDefault, con);
                clear.ExecuteNonQuery();
            }

            using SqlCommand cmd = new SqlCommand(SalaryRuleMasterQueries.Update, con);
            cmd.Parameters.AddWithValue("@RuleId", model.RuleId);
            BindParams(cmd, model);
            cmd.ExecuteNonQuery();
        }

        public void SetDefault(int ruleId)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using (SqlCommand clear = new SqlCommand(SalaryRuleMasterQueries.ClearDefault, con))
                clear.ExecuteNonQuery();
            using SqlCommand cmd = new SqlCommand(SalaryRuleMasterQueries.SetDefault, con);
            cmd.Parameters.AddWithValue("@RuleId", ruleId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(SalaryRuleMasterQueries.Delete, con);
            cmd.Parameters.AddWithValue("@RuleId", id);
            cmd.ExecuteNonQuery();
        }

        private static void BindParams(SqlCommand cmd, SalaryRuleMaster model)
        {
            cmd.Parameters.AddWithValue("@RuleName", model.RuleName);
            cmd.Parameters.AddWithValue("@WorkingDaysPerMonth", model.WorkingDaysPerMonth);
            cmd.Parameters.AddWithValue("@AbsentDeductionPerDay", model.AbsentDeductionPerDay);
            cmd.Parameters.AddWithValue("@HalfDayDeductionFactor", model.HalfDayDeductionFactor);
            cmd.Parameters.AddWithValue("@LeaveIsPaid", model.LeaveIsPaid);
            cmd.Parameters.AddWithValue("@LateGraceMinutes", model.LateGraceMinutes);
            cmd.Parameters.AddWithValue("@ShiftStartTime", (object?)model.ShiftStartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShiftEndTime", (object?)model.ShiftEndTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EarlyLeaveGraceMinutes", model.EarlyLeaveGraceMinutes);
            cmd.Parameters.AddWithValue("@EnableSandwichRule", model.EnableSandwichRule);
            cmd.Parameters.AddWithValue("@WeeklyOffDays", (object?)model.WeeklyOffDays ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LateCountsAsHalfDay", model.LateCountsAsHalfDay);
            cmd.Parameters.AddWithValue("@EarlyLeaveCountsAsHalfDay", model.EarlyLeaveCountsAsHalfDay);
            cmd.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
            cmd.Parameters.AddWithValue("@IsDefault", model.IsDefault);
        }

        private static SalaryRuleMaster Map(SqlDataReader dr)
        {
            var rule = new SalaryRuleMaster
            {
                RuleId = Convert.ToInt32(dr["RuleId"]),
                RuleName = dr["RuleName"]?.ToString() ?? "",
                WorkingDaysPerMonth = Convert.ToInt32(dr["WorkingDaysPerMonth"]),
                AbsentDeductionPerDay = Convert.ToBoolean(dr["AbsentDeductionPerDay"]),
                HalfDayDeductionFactor = Convert.ToDecimal(dr["HalfDayDeductionFactor"]),
                LeaveIsPaid = Convert.ToBoolean(dr["LeaveIsPaid"]),
                LateGraceMinutes = Convert.ToInt32(dr["LateGraceMinutes"]),
                Description = dr["Description"]?.ToString(),
                IsActive = Convert.ToBoolean(dr["IsActive"]),
                IsDefault = Convert.ToBoolean(dr["IsDefault"]),
                CreatedDate = Convert.ToDateTime(dr["CreatedDate"])
            };

            if (HasColumn(dr, "ShiftStartTime"))
                rule.ShiftStartTime = TimeFormatHelper.ReadTime(dr["ShiftStartTime"]);
            if (HasColumn(dr, "ShiftEndTime"))
                rule.ShiftEndTime = TimeFormatHelper.ReadTime(dr["ShiftEndTime"]);
            if (HasColumn(dr, "EarlyLeaveGraceMinutes"))
                rule.EarlyLeaveGraceMinutes = Convert.ToInt32(dr["EarlyLeaveGraceMinutes"]);
            if (HasColumn(dr, "EnableSandwichRule"))
                rule.EnableSandwichRule = Convert.ToBoolean(dr["EnableSandwichRule"]);
            if (HasColumn(dr, "WeeklyOffDays"))
                rule.WeeklyOffDays = dr["WeeklyOffDays"]?.ToString();
            if (HasColumn(dr, "LateCountsAsHalfDay"))
                rule.LateCountsAsHalfDay = Convert.ToBoolean(dr["LateCountsAsHalfDay"]);
            if (HasColumn(dr, "EarlyLeaveCountsAsHalfDay"))
                rule.EarlyLeaveCountsAsHalfDay = Convert.ToBoolean(dr["EarlyLeaveCountsAsHalfDay"]);

            return rule;
        }

        private static bool HasColumn(SqlDataReader dr, string name)
        {
            for (int i = 0; i < dr.FieldCount; i++)
                if (dr.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}

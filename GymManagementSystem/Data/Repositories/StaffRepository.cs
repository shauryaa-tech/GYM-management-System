using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class StaffRepository
    {
        private readonly DbHelper _db;

        public StaffRepository(DbHelper db)
        {
            _db = db;
        }

        public List<StaffMaster> GetAll()
        {
            List<StaffMaster> list = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    StaffQueries.GetAll,
                    con);

            SqlDataReader dr =
                cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(
                    new StaffMaster
                    {
                        StaffId =
                            Convert.ToInt32(dr["StaffId"]),

                        StaffName =
                            dr["StaffName"].ToString(),

                        Gender =
                            dr["Gender"].ToString(),

                        MobileNo =
                            dr["MobileNo"].ToString(),

                        Email =
                            dr["Email"].ToString(),

                        Designation =
                            dr["Designation"].ToString(),

                        Specializations = HasColumn(dr, "Specializations")
                            ? dr["Specializations"]?.ToString()
                            : null,

                        ExperienceYears = HasColumn(dr, "ExperienceYears") && dr["ExperienceYears"] != DBNull.Value
                            ? Convert.ToInt32(dr["ExperienceYears"])
                            : null,

                        Salary =
                            Convert.ToDecimal(dr["Salary"]),

                        JoiningDate =
                            Convert.ToDateTime(
                                dr["JoiningDate"]),

                        Address =
                            dr["Address"].ToString(),

                        IsActive =
                            Convert.ToBoolean(
                                dr["IsActive"]),

                        RoleId =
                            Convert.ToInt32(
                                dr["RoleId"]),

                        RoleName =
                            dr["RoleName"].ToString()
                    });

                MapProfileColumns(dr, list[^1]);
            }

            return list;
        }
        public StaffMaster GetById(int id)
        {
            StaffMaster staff = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    StaffQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue(
                "@StaffId",
                id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                staff.StaffId =
                    Convert.ToInt32(
                        dr["StaffId"]);

                staff.StaffName =
                    dr["StaffName"].ToString();

                staff.Gender =
                    dr["Gender"].ToString();

                staff.MobileNo =
                    dr["MobileNo"].ToString();

                staff.Email =
                    dr["Email"].ToString();

                staff.Designation =
                    dr["Designation"].ToString();

                staff.Specializations = HasColumn(dr, "Specializations")
                    ? dr["Specializations"]?.ToString()
                    : null;

                staff.ExperienceYears = HasColumn(dr, "ExperienceYears") && dr["ExperienceYears"] != DBNull.Value
                    ? Convert.ToInt32(dr["ExperienceYears"])
                    : null;

                staff.Salary =
                    Convert.ToDecimal(
                        dr["Salary"]);

                staff.JoiningDate =
                    Convert.ToDateTime(
                        dr["JoiningDate"]);

                staff.Address =
                    dr["Address"].ToString();

                staff.IsActive =
                    Convert.ToBoolean(
                        dr["IsActive"]);

                staff.RoleId =
                    Convert.ToInt32(
                        dr["RoleId"]);

                MapProfileColumns(dr, staff);
            }

            return staff;
        }

        public StaffMaster? GetByStaffCode(string staffCode)
        {
            if (string.IsNullOrWhiteSpace(staffCode))
                return null;

            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffQueries.GetByStaffCode, con);
            cmd.Parameters.AddWithValue("@StaffCode", staffCode.Trim());
            using SqlDataReader dr = cmd.ExecuteReader();
            if (!dr.Read())
                return null;

            var staff = new StaffMaster
            {
                StaffId = Convert.ToInt32(dr["StaffId"]),
                StaffName = dr["StaffName"]?.ToString() ?? "",
                Gender = dr["Gender"]?.ToString() ?? "",
                MobileNo = dr["MobileNo"]?.ToString() ?? "",
                Email = dr["Email"]?.ToString() ?? "",
                Designation = dr["Designation"]?.ToString() ?? "",
                Salary = Convert.ToDecimal(dr["Salary"]),
                JoiningDate = Convert.ToDateTime(dr["JoiningDate"]),
                Address = dr["Address"]?.ToString() ?? "",
                IsActive = Convert.ToBoolean(dr["IsActive"]),
                RoleId = Convert.ToInt32(dr["RoleId"]),
                RoleName = HasColumn(dr, "RoleName") ? dr["RoleName"]?.ToString() : null
            };
            MapProfileColumns(dr, staff);
            return staff;
        }

        public bool IsStaffCodeTaken(string staffCode, int? excludeStaffId = null)
        {
            if (string.IsNullOrWhiteSpace(staffCode))
                return false;

            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffQueries.IsStaffCodeTaken, con);
            cmd.Parameters.AddWithValue("@StaffCode", staffCode.Trim());
            cmd.Parameters.AddWithValue("@ExcludeStaffId", (object?)excludeStaffId ?? DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public string NormalizeStaffCode(StaffMaster staff)
        {
            if (!string.IsNullOrWhiteSpace(staff.StaffCode))
                return staff.StaffCode.Trim().ToUpperInvariant();

            return $"EMP{staff.StaffId:D3}";
        }

        public void PrepareStaffCodeForSave(StaffMaster staff)
        {
            if (!string.IsNullOrWhiteSpace(staff.StaffCode))
            {
                staff.StaffCode = staff.StaffCode.Trim().ToUpperInvariant();
                return;
            }

            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(StaffId), 0) + 1 FROM StaffMasters", con);
            var nextId = Convert.ToInt32(cmd.ExecuteScalar());
            staff.StaffCode = $"EMP{nextId:D3}";
        }

        public void Insert(StaffMaster staff)
        {
            PrepareStaffCodeForSave(staff);

            try
            {
                InsertWithProfile(staff);
            }
            catch (SqlException)
            {
                try
                {
                    InsertWithExpertise(staff);
                }
                catch (SqlException)
                {
                    InsertBasic(staff);
                }
            }
        }

        public void Update(StaffMaster staff)
        {
            if (!string.IsNullOrWhiteSpace(staff.StaffCode))
                staff.StaffCode = staff.StaffCode.Trim().ToUpperInvariant();
            else
                staff.StaffCode = NormalizeStaffCode(staff);

            try
            {
                UpdateWithProfile(staff);
            }
            catch (SqlException)
            {
                try
                {
                    UpdateWithExpertise(staff);
                }
                catch (SqlException)
                {
                    UpdateBasic(staff);
                }
            }
        }

        private void InsertWithProfile(StaffMaster staff)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffQueries.InsertWithProfile, con);
            AddProfileStaffParameters(cmd, staff);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());
            staff.StaffId = newId;
            if (string.IsNullOrWhiteSpace(staff.StaffCode))
                staff.StaffCode = $"EMP{newId:D3}";
        }

        private void UpdateWithProfile(StaffMaster staff)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffQueries.UpdateWithProfile, con);
            cmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
            AddProfileStaffParameters(cmd, staff);
            cmd.ExecuteNonQuery();
        }

        private static void AddProfileStaffParameters(SqlCommand cmd, StaffMaster staff)
        {
            AddBasicStaffParameters(cmd, staff);
            cmd.Parameters.AddWithValue("@StaffCode", (object?)staff.StaffCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Specializations", (object?)staff.Specializations ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceYears", (object?)staff.ExperienceYears ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankName", (object?)staff.BankName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BankAccountNo", (object?)staff.BankAccountNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IfscCode", (object?)staff.IfscCode ?? DBNull.Value);
        }

        private static void MapProfileColumns(SqlDataReader dr, StaffMaster staff)
        {
            if (HasColumn(dr, "StaffCode"))
                staff.StaffCode = dr["StaffCode"]?.ToString();
            if (HasColumn(dr, "BankName"))
                staff.BankName = dr["BankName"]?.ToString();
            if (HasColumn(dr, "BankAccountNo"))
                staff.BankAccountNo = dr["BankAccountNo"]?.ToString();
            if (HasColumn(dr, "IfscCode"))
                staff.IfscCode = dr["IfscCode"]?.ToString();
        }

        private void InsertBasic(StaffMaster staff)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffQueries.Insert, con);
            AddBasicStaffParameters(cmd, staff);
            cmd.ExecuteNonQuery();
        }

        private void InsertWithExpertise(StaffMaster staff)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffQueries.InsertWithExpertise, con);
            AddBasicStaffParameters(cmd, staff);
            cmd.Parameters.AddWithValue("@Specializations", (object?)staff.Specializations ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceYears", (object?)staff.ExperienceYears ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private void UpdateBasic(StaffMaster staff)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffQueries.Update, con);
            cmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
            AddBasicStaffParameters(cmd, staff);
            cmd.ExecuteNonQuery();
        }

        private void UpdateWithExpertise(StaffMaster staff)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffQueries.UpdateWithExpertise, con);
            cmd.Parameters.AddWithValue("@StaffId", staff.StaffId);
            AddBasicStaffParameters(cmd, staff);
            cmd.Parameters.AddWithValue("@Specializations", (object?)staff.Specializations ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceYears", (object?)staff.ExperienceYears ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static void AddBasicStaffParameters(SqlCommand cmd, StaffMaster staff)
        {
            cmd.Parameters.AddWithValue("@StaffName", staff.StaffName);
            cmd.Parameters.AddWithValue("@Gender", staff.Gender);
            cmd.Parameters.AddWithValue("@MobileNo", staff.MobileNo);
            cmd.Parameters.AddWithValue("@Email", staff.Email ?? "");
            cmd.Parameters.AddWithValue("@Designation", staff.Designation ?? "");
            cmd.Parameters.AddWithValue("@Salary", staff.Salary);
            cmd.Parameters.AddWithValue("@JoiningDate", staff.JoiningDate);
            cmd.Parameters.AddWithValue("@Address", staff.Address ?? "");
            cmd.Parameters.AddWithValue("@IsActive", staff.IsActive);
            cmd.Parameters.AddWithValue("@RoleId", staff.RoleId);
        }

        public void Delete(int id)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    StaffQueries.Delete,
                    con);

            cmd.Parameters.AddWithValue(
                "@StaffId",
                id);

            cmd.ExecuteNonQuery();
        }

        public List<StaffMaster> GetTrainers()
        {
            List<StaffMaster> list = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(@"
SELECT
    S.StaffId,
    S.StaffName
FROM StaffMasters S
INNER JOIN RoleMaster R
ON S.RoleId = R.RoleId
WHERE
    S.IsActive = 1
    AND R.RoleName='Trainer'
ORDER BY S.StaffName", con);

            SqlDataReader dr =
                cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new StaffMaster
                {
                    StaffId =
                        Convert.ToInt32(dr["StaffId"]),

                    StaffName =
                        dr["StaffName"].ToString()!
                });
            }

            return list;
        }

        public List<StaffMaster> GetTrainersWithExpertise()
        {
            var list = new List<StaffMaster>();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            try
            {
                using SqlCommand cmd = new SqlCommand(StaffQueries.GetTrainersWithExpertise, con);
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new StaffMaster
                    {
                        StaffId = Convert.ToInt32(dr["StaffId"]),
                        StaffName = dr["StaffName"]?.ToString() ?? "",
                        Specializations = HasColumn(dr, "Specializations") ? dr["Specializations"]?.ToString() : null,
                        ExperienceYears = HasColumn(dr, "ExperienceYears") && dr["ExperienceYears"] != DBNull.Value
                            ? Convert.ToInt32(dr["ExperienceYears"])
                            : null
                    });
                }
            }
            catch (SqlException)
            {
                return GetTrainers();
            }

            return list;
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
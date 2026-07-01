using GymManagement.Helpers;
using GymManagement.Models;
using GymManagement.Data.Queries;
using GymManagement.ViewModels;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class StaffAttendanceRepository
    {
        private readonly DbHelper _db;

        public StaffAttendanceRepository(DbHelper db)
        {
            _db = db;
        }

        public List<StaffAttendance> GetAll(string? search, string? staffId, string? status, string? fromDate, string? toDate)
        {
            List<StaffAttendance> attendances = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = StaffAttendanceQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (S.StaffName LIKE @Search OR SA.Remarks LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(staffId))
            {
                query += " AND SA.StaffId=@StaffId";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query += " AND SA.Status=@Status";
            }

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                query += " AND SA.AttendanceDate >= @FromDate";
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                query += " AND SA.AttendanceDate <= @ToDate";
            }

            query += " ORDER BY SA.AttendanceDate DESC, SA.AttendanceId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(staffId))
            {
                cmd.Parameters.AddWithValue("@StaffId", staffId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@Status", status);
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
                attendances.Add(MapStaffAttendance(dr));
            }

            return attendances;
        }

        public bool ExistsForStaffDate(int staffId, DateTime date, int? excludeId = null)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            var sql = @"
                SELECT COUNT(1) FROM StaffAttendances
                WHERE StaffId = @StaffId
                  AND CAST(AttendanceDate AS DATE) = CAST(@AttendanceDate AS DATE)";

            if (excludeId.HasValue)
                sql += " AND AttendanceId <> @ExcludeId";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@AttendanceDate", date.Date);
            if (excludeId.HasValue)
                cmd.Parameters.AddWithValue("@ExcludeId", excludeId.Value);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Insert(StaffAttendance model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.Insert, con);

            cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
            cmd.Parameters.AddWithValue("@AttendanceDate", model.AttendanceDate);
            cmd.Parameters.AddWithValue("@CheckInTime", (object?)model.CheckInTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOutTime", (object?)model.CheckOutTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)model.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public StaffAttendance GetById(int id)
        {
            StaffAttendance model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetById, con);
            cmd.Parameters.AddWithValue("@AttendanceId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model = MapStaffAttendance(dr);
            }

            return model;
        }

        public void Update(StaffAttendance model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.Update, con);

            cmd.Parameters.AddWithValue("@AttendanceId", model.AttendanceId);
            cmd.Parameters.AddWithValue("@StaffId", model.StaffId);
            cmd.Parameters.AddWithValue("@AttendanceDate", model.AttendanceDate);
            cmd.Parameters.AddWithValue("@CheckInTime", (object?)model.CheckInTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOutTime", (object?)model.CheckOutTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)model.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.Delete, con);
            cmd.Parameters.AddWithValue("@AttendanceId", id);

            cmd.ExecuteNonQuery();
        }

        public List<StaffMaster> GetActiveStaff()
        {
            List<StaffMaster> staffList = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetActiveStaff, con);

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

        public List<StaffTodayAttendanceRow> GetTodayBoard() => GetBoardForDate(DateTime.Today);

        public List<StaffTodayAttendanceRow> GetBoardForDate(DateTime attendanceDate)
        {
            var list = new List<StaffTodayAttendanceRow>();
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetBoardForDate, con);
            cmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate.Date);
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new StaffTodayAttendanceRow
                {
                    StaffId = Convert.ToInt32(dr["StaffId"]),
                    StaffName = dr["StaffName"]?.ToString() ?? "",
                    Designation = dr["Designation"]?.ToString(),
                    AttendanceId = dr["AttendanceId"] == DBNull.Value ? null : Convert.ToInt32(dr["AttendanceId"]),
                    DayStatus = string.IsNullOrWhiteSpace(dr["DayStatus"]?.ToString()) ? "NotMarked" : dr["DayStatus"]!.ToString()!,
                    CheckInTime = ReadTime(dr["CheckInTime"]),
                    CheckOutTime = ReadTime(dr["CheckOutTime"]),
                    Remarks = dr["Remarks"]?.ToString()
                });
            }
            return list;
        }

        public int? GetTodayAttendanceId(int staffId)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetByStaffAndDate, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@AttendanceDate", DateTime.Today);
            var result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        public void QuickCheckIn(int staffId)
        {
            var attendanceId = GetTodayAttendanceId(staffId);
            var checkInTime = DateTime.Now.TimeOfDay;

            using SqlConnection con = _db.GetConnection();
            con.Open();

            if (attendanceId.HasValue)
            {
                using SqlCommand cmd = new SqlCommand(@"
                    UPDATE StaffAttendances
                    SET CheckInTime = @CheckInTime,
                        Status = 'Present',
                        CheckOutTime = NULL
                    WHERE AttendanceId = @AttendanceId
                      AND (CheckInTime IS NULL OR Status IN ('Absent', 'Leave', 'HalfDay'))", con);
                cmd.Parameters.AddWithValue("@AttendanceId", attendanceId.Value);
                cmd.Parameters.AddWithValue("@CheckInTime", checkInTime);
                cmd.ExecuteNonQuery();
                return;
            }

            using SqlCommand insertCmd = new SqlCommand(StaffAttendanceQueries.QuickCheckIn, con);
            insertCmd.Parameters.AddWithValue("@StaffId", staffId);
            insertCmd.Parameters.AddWithValue("@CheckInTime", checkInTime);
            insertCmd.ExecuteNonQuery();
        }

        public void QuickCheckOut(int staffId)
        {
            var attendanceId = GetTodayAttendanceId(staffId);
            if (!attendanceId.HasValue) return;

            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.UpdateCheckOut, con);
            cmd.Parameters.AddWithValue("@AttendanceId", attendanceId.Value);
            cmd.Parameters.AddWithValue("@CheckOutTime", DateTime.Now.TimeOfDay);
            cmd.ExecuteNonQuery();
        }

        public void MarkTodayStatus(int staffId, string status, string? remarks = null)
        {
            var attendanceId = GetTodayAttendanceId(staffId);
            using SqlConnection con = _db.GetConnection();
            con.Open();

            if (attendanceId.HasValue)
            {
                using SqlCommand cmd = new SqlCommand(@"
                    UPDATE StaffAttendances SET Status=@Status, Remarks=@Remarks WHERE AttendanceId=@Id", con);
                cmd.Parameters.AddWithValue("@Id", attendanceId.Value);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Remarks", (object?)remarks ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            else
            {
                using SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO StaffAttendances (StaffId, AttendanceDate, Status, Remarks)
                    VALUES (@StaffId, CAST(GETDATE() AS DATE), @Status, @Remarks)", con);
                cmd.Parameters.AddWithValue("@StaffId", staffId);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Remarks", (object?)remarks ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public (int PresentDays, int AbsentDays, int LeaveDays, int HalfDays, int TotalMarked) GetMonthlyCounts(int staffId, int month, int year)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetMonthlyCounts, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@Month", month);
            cmd.Parameters.AddWithValue("@Year", year);
            using SqlDataReader dr = cmd.ExecuteReader();
            if (!dr.Read()) return (0, 0, 0, 0, 0);
            return (
                dr["PresentDays"] == DBNull.Value ? 0 : Convert.ToInt32(dr["PresentDays"]),
                dr["AbsentDays"] == DBNull.Value ? 0 : Convert.ToInt32(dr["AbsentDays"]),
                dr["LeaveDays"] == DBNull.Value ? 0 : Convert.ToInt32(dr["LeaveDays"]),
                dr["HalfDays"] == DBNull.Value ? 0 : Convert.ToInt32(dr["HalfDays"]),
                dr["TotalMarked"] == DBNull.Value ? 0 : Convert.ToInt32(dr["TotalMarked"])
            );
        }

        public List<StaffAttendanceDayRow> GetMonthlyAttendance(int staffId, int month, int year)
        {
            var list = new List<StaffAttendanceDayRow>();
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetMonthlyAttendance, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@Month", month);
            cmd.Parameters.AddWithValue("@Year", year);
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new StaffAttendanceDayRow
                {
                    Date = Convert.ToDateTime(dr["AttendanceDate"]),
                    Status = dr["Status"]?.ToString() ?? "Absent",
                    CheckInTime = ReadTime(dr["CheckInTime"]),
                    CheckOutTime = ReadTime(dr["CheckOutTime"])
                });
            }
            return list;
        }

        public List<StaffHistoryDayRow> GetByStaffAndDateRange(int staffId, DateTime fromDate, DateTime toDate)
        {
            var list = new List<StaffHistoryDayRow>();
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetByStaffDateRange, con);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
            cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new StaffHistoryDayRow
                {
                    AttendanceId = Convert.ToInt32(dr["AttendanceId"]),
                    AttendanceDate = Convert.ToDateTime(dr["AttendanceDate"]),
                    Status = dr["Status"]?.ToString() ?? "Absent",
                    CheckInTime = ReadTime(dr["CheckInTime"]),
                    CheckOutTime = ReadTime(dr["CheckOutTime"]),
                    Remarks = dr["Remarks"]?.ToString()
                });
            }
            return list;
        }

        public List<StaffMonthlySummaryRow> GetMonthlySummaryAllStaff(int month, int year)
        {
            var list = new List<StaffMonthlySummaryRow>();
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(StaffAttendanceQueries.GetMonthlySummaryAllStaff, con);
            cmd.Parameters.AddWithValue("@Month", month);
            cmd.Parameters.AddWithValue("@Year", year);
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new StaffMonthlySummaryRow
                {
                    StaffId = Convert.ToInt32(dr["StaffId"]),
                    StaffName = dr["StaffName"]?.ToString() ?? "",
                    Designation = dr["Designation"]?.ToString(),
                    BasicSalary = Convert.ToDecimal(dr["Salary"]),
                    PresentDays = Convert.ToInt32(dr["PresentDays"]),
                    AbsentDays = Convert.ToInt32(dr["AbsentDays"]),
                    LeaveDays = Convert.ToInt32(dr["LeaveDays"]),
                    HalfDays = Convert.ToInt32(dr["HalfDays"]),
                    TotalMarked = Convert.ToInt32(dr["TotalMarked"])
                });
            }
            return list;
        }

        private static StaffAttendance MapStaffAttendance(SqlDataReader dr)
        {
            return new StaffAttendance
            {
                AttendanceId = Convert.ToInt32(dr["AttendanceId"]),
                StaffId = Convert.ToInt32(dr["StaffId"]),
                AttendanceDate = Convert.ToDateTime(dr["AttendanceDate"]),
                CheckInTime = ReadTime(dr["CheckInTime"]),
                CheckOutTime = ReadTime(dr["CheckOutTime"]),
                Status = dr["Status"]?.ToString(),
                Remarks = dr["Remarks"]?.ToString(),
                StaffName = dr["StaffName"]?.ToString(),
                Designation = dr["Designation"]?.ToString()
            };
        }

        private static TimeSpan? ReadTime(object? value) => TimeFormatHelper.ReadTime(value);
    }
}

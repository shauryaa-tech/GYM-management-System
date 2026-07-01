using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class AttendanceRepository
    {
        private readonly DbHelper _db;

        public AttendanceRepository(DbHelper db)
        {
            _db = db;
        }

        public List<Attendance> GetAll(DateTime? filterDate)
        {
            List<Attendance> attendances = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = AttendanceQueries.GetAll;

            if (filterDate.HasValue)
            {
                query += " AND A.AttendanceDate=@AttendanceDate";
            }
            else
            {
                query += " AND A.AttendanceDate=CAST(GETDATE() AS DATE)";
            }

            query += " ORDER BY A.AttendanceId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (filterDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@AttendanceDate", filterDate.Value.Date);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                attendances.Add(new Attendance
                {
                    AttendanceId = Convert.ToInt32(dr["AttendanceId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    AttendanceDate = Convert.ToDateTime(dr["AttendanceDate"]),
                    CheckInTime = dr["CheckInTime"] == DBNull.Value ? null : (TimeSpan)dr["CheckInTime"],
                    CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? null : (TimeSpan)dr["CheckOutTime"],
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString()
                });
            }

            return attendances;
        }

        public List<Attendance> GetByMemberAndDateRange(int memberId, DateTime fromDate, DateTime toDate)
        {
            var attendances = new List<Attendance>();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            using SqlCommand cmd = new SqlCommand(AttendanceQueries.GetByMemberDateRange, con);
            cmd.Parameters.AddWithValue("@MemberId", memberId);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
            cmd.Parameters.AddWithValue("@ToDate", toDate.Date);

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                attendances.Add(new Attendance
                {
                    AttendanceId = Convert.ToInt32(dr["AttendanceId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    AttendanceDate = Convert.ToDateTime(dr["AttendanceDate"]),
                    CheckInTime = dr["CheckInTime"] == DBNull.Value ? null : (TimeSpan)dr["CheckInTime"],
                    CheckOutTime = dr["CheckOutTime"] == DBNull.Value ? null : (TimeSpan)dr["CheckOutTime"],
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString()
                });
            }

            return attendances;
        }

        public void Insert(Attendance model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AttendanceQueries.Insert, con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@AttendanceDate", model.AttendanceDate);
            cmd.Parameters.AddWithValue("@CheckInTime", (object?)model.CheckInTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOutTime", (object?)model.CheckOutTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public Attendance GetById(int id)
        {
            Attendance model = new();

            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    AttendanceQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue(
                "@AttendanceId",
                id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                model.AttendanceId =
                    Convert.ToInt32(
                        dr["AttendanceId"]);

                model.MemberId =
                    Convert.ToInt32(
                        dr["MemberId"]);

                model.AttendanceDate =
                    Convert.ToDateTime(
                        dr["AttendanceDate"]);

                model.CheckInTime =
                    dr["CheckInTime"] == DBNull.Value
                    ? null
                    : (TimeSpan?)dr["CheckInTime"];

                model.CheckOutTime =
                    dr["CheckOutTime"] == DBNull.Value
                    ? null
                    : (TimeSpan?)dr["CheckOutTime"];

                model.Remarks =
                    dr["Remarks"]?.ToString();
            }

            return model;
        }

        public void Update(Attendance model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    AttendanceQueries.Update,
                    con);

            cmd.Parameters.AddWithValue(
                "@AttendanceId",
                model.AttendanceId);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                model.MemberId);

            cmd.Parameters.AddWithValue(
                "@AttendanceDate",
                model.AttendanceDate);

            cmd.Parameters.AddWithValue(
                "@CheckInTime",
                (object?)model.CheckInTime ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@CheckOutTime",
                (object?)model.CheckOutTime ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Remarks",
                (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(AttendanceQueries.Delete, con);
            cmd.Parameters.AddWithValue("@AttendanceId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

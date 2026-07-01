using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class ClassBookingRepository
    {
        private readonly DbHelper _db;

        public ClassBookingRepository(DbHelper db)
        {
            _db = db;
        }

        public List<ClassBooking> GetAll(string? search, string? memberId, string? classId, string? status)
        {
            List<ClassBooking> bookings = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = ClassBookingQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (M.MemberName LIKE @Search OR M.MemberCode LIKE @Search OR CM.ClassName LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(memberId))
            {
                query += " AND CB.MemberId=@MemberId";
            }

            if (!string.IsNullOrWhiteSpace(classId))
            {
                query += " AND CB.ClassId=@ClassId";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query += " AND CB.Status=@Status";
            }

            query += " ORDER BY CB.BookingDate DESC, CB.BookingId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(memberId))
            {
                cmd.Parameters.AddWithValue("@MemberId", memberId);
            }

            if (!string.IsNullOrWhiteSpace(classId))
            {
                cmd.Parameters.AddWithValue("@ClassId", classId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue("@Status", status);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                bookings.Add(new ClassBooking
                {
                    BookingId = Convert.ToInt32(dr["BookingId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    ClassId = Convert.ToInt32(dr["ClassId"]),
                    BookingDate = Convert.ToDateTime(dr["BookingDate"]),
                    Status = dr["Status"]?.ToString() ?? "Confirmed",
                    Amount = dr["Amount"] == DBNull.Value ? null : Convert.ToDecimal(dr["Amount"]),
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    ClassName = dr["ClassName"]?.ToString(),
                    TrainerName = dr["TrainerName"]?.ToString(),
                    StartTime = dr["StartTime"] == DBNull.Value ? null : (TimeSpan)dr["StartTime"],
                    EndTime = dr["EndTime"] == DBNull.Value ? null : (TimeSpan)dr["EndTime"]
                });
            }

            return bookings;
        }

        public void Insert(ClassBooking model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassBookingQueries.Insert, con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@ClassId", model.ClassId);
            cmd.Parameters.AddWithValue("@BookingDate", model.BookingDate);
            cmd.Parameters.AddWithValue("@Status", model.Status);
            cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public ClassBooking GetById(int id)
        {
            ClassBooking model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassBookingQueries.GetById, con);
            cmd.Parameters.AddWithValue("@BookingId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.BookingId = Convert.ToInt32(dr["BookingId"]);
                model.MemberId = Convert.ToInt32(dr["MemberId"]);
                model.ClassId = Convert.ToInt32(dr["ClassId"]);
                model.BookingDate = Convert.ToDateTime(dr["BookingDate"]);
                model.Status = dr["Status"]?.ToString() ?? "Confirmed";
                model.Amount = dr["Amount"] == DBNull.Value ? null : Convert.ToDecimal(dr["Amount"]);
                model.Remarks = dr["Remarks"]?.ToString();
                model.MemberName = dr["MemberName"]?.ToString();
                model.MemberCode = dr["MemberCode"]?.ToString();
                model.ClassName = dr["ClassName"]?.ToString();
                model.TrainerName = dr["TrainerName"]?.ToString();
                model.StartTime = dr["StartTime"] == DBNull.Value ? null : (TimeSpan)dr["StartTime"];
                model.EndTime = dr["EndTime"] == DBNull.Value ? null : (TimeSpan)dr["EndTime"];
            }

            return model;
        }

        public void Update(ClassBooking model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassBookingQueries.Update, con);

            cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@ClassId", model.ClassId);
            cmd.Parameters.AddWithValue("@BookingDate", model.BookingDate);
            cmd.Parameters.AddWithValue("@Status", model.Status);
            cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassBookingQueries.Delete, con);
            cmd.Parameters.AddWithValue("@BookingId", id);

            cmd.ExecuteNonQuery();
        }

        public int GetBookingCount(int classId, DateTime bookingDate)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassBookingQueries.GetBookingCountByClass, con);
            cmd.Parameters.AddWithValue("@ClassId", classId);
            cmd.Parameters.AddWithValue("@BookingDate", bookingDate);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<ClassMaster> GetActiveClasses()
        {
            List<ClassMaster> classes = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassBookingQueries.GetActiveClassesForBooking, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                classes.Add(new ClassMaster
                {
                    ClassId = Convert.ToInt32(dr["ClassId"]),
                    ClassName = dr["ClassName"]?.ToString() ?? "",
                    TrainerId = dr["TrainerId"] == DBNull.Value ? null : Convert.ToInt32(dr["TrainerId"]),
                    Schedule = dr["Schedule"]?.ToString(),
                    StartTime = dr["StartTime"] == DBNull.Value ? null : (TimeSpan)dr["StartTime"],
                    EndTime = dr["EndTime"] == DBNull.Value ? null : (TimeSpan)dr["EndTime"],
                    MaxCapacity = dr["MaxCapacity"] == DBNull.Value ? null : Convert.ToInt32(dr["MaxCapacity"]),
                    Amount = dr["Amount"] == DBNull.Value ? null : Convert.ToDecimal(dr["Amount"])
                });
            }

            return classes;
        }
    }
}
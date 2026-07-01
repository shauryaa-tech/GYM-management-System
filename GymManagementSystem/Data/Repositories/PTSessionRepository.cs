using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class PTSessionRepository
    {
        private readonly DbHelper _db;

        public PTSessionRepository(DbHelper db)
        {
            _db = db;
        }

        public List<PTSession> GetAll(string? search)
        {
            List<PTSession> sessions = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = PTSessionQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (M.MemberName LIKE @Search OR M.MemberCode LIKE @Search OR S.StaffName LIKE @Search)";
            }

            query += " ORDER BY P.SessionDate DESC, P.SessionId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                sessions.Add(new PTSession
                {
                    SessionId = Convert.ToInt32(dr["SessionId"]),
                    MemberId = Convert.ToInt32(dr["MemberId"]),
                    TrainerId = Convert.ToInt32(dr["TrainerId"]),
                    SessionDate = Convert.ToDateTime(dr["SessionDate"]),
                    StartTime = (TimeSpan)dr["StartTime"],
                    EndTime = (TimeSpan)dr["EndTime"],
                    Status = dr["Status"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString(),
                    MemberName = dr["MemberName"]?.ToString(),
                    MemberCode = dr["MemberCode"]?.ToString(),
                    TrainerName = dr["TrainerName"]?.ToString()
                });
            }

            return sessions;
        }

        public void Insert(PTSession model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(PTSessionQueries.Insert, con);

            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@TrainerId", model.TrainerId);
            cmd.Parameters.AddWithValue("@SessionDate", model.SessionDate);
            cmd.Parameters.AddWithValue("@StartTime", model.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", model.EndTime);
            cmd.Parameters.AddWithValue("@Status", (object?)model.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public PTSession GetById(int id)
        {
            PTSession model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(PTSessionQueries.GetById, con);
            cmd.Parameters.AddWithValue("@SessionId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.SessionId = Convert.ToInt32(dr["SessionId"]);
                model.MemberId = Convert.ToInt32(dr["MemberId"]);
                model.TrainerId = Convert.ToInt32(dr["TrainerId"]);
                model.SessionDate = Convert.ToDateTime(dr["SessionDate"]);
                model.StartTime = (TimeSpan)dr["StartTime"];
                model.EndTime = (TimeSpan)dr["EndTime"];
                model.Status = dr["Status"]?.ToString();
                model.Remarks = dr["Remarks"]?.ToString();
            }

            return model;
        }

        public void Update(PTSession model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(PTSessionQueries.Update, con);

            cmd.Parameters.AddWithValue("@SessionId", model.SessionId);
            cmd.Parameters.AddWithValue("@MemberId", model.MemberId);
            cmd.Parameters.AddWithValue("@TrainerId", model.TrainerId);
            cmd.Parameters.AddWithValue("@SessionDate", model.SessionDate);
            cmd.Parameters.AddWithValue("@StartTime", model.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", model.EndTime);
            cmd.Parameters.AddWithValue("@Status", (object?)model.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(PTSessionQueries.Delete, con);
            cmd.Parameters.AddWithValue("@SessionId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

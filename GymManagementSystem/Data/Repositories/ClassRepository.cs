using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class ClassRepository
    {
        private readonly DbHelper _db;

        public ClassRepository(DbHelper db)
        {
            _db = db;
        }

        public List<ClassMaster> GetAll(string? search, string? trainerId)
        {
            List<ClassMaster> classes = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = ClassQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND C.ClassName LIKE @Search";
            }

            if (!string.IsNullOrWhiteSpace(trainerId))
            {
                query += " AND C.TrainerId=@TrainerId";
            }

            query += " ORDER BY C.ClassId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(trainerId))
            {
                cmd.Parameters.AddWithValue("@TrainerId", trainerId);
            }

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
                    Amount = dr["Amount"] == DBNull.Value ? null : Convert.ToDecimal(dr["Amount"]),
                    IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]),
                    TrainerName = dr["TrainerName"]?.ToString()
                });
            }

            return classes;
        }

        public void Insert(ClassMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassQueries.Insert, con);

            cmd.Parameters.AddWithValue("@ClassName", model.ClassName);
            cmd.Parameters.AddWithValue("@TrainerId", (object?)model.TrainerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Schedule", (object?)model.Schedule ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartTime", (object?)model.StartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndTime", (object?)model.EndTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxCapacity", (object?)model.MaxCapacity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public ClassMaster GetById(int id)
        {
            ClassMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassQueries.GetById, con);
            cmd.Parameters.AddWithValue("@ClassId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.ClassId = Convert.ToInt32(dr["ClassId"]);
                model.ClassName = dr["ClassName"]?.ToString() ?? "";
                model.TrainerId = dr["TrainerId"] == DBNull.Value ? null : Convert.ToInt32(dr["TrainerId"]);
                model.Schedule = dr["Schedule"]?.ToString();
                model.StartTime = dr["StartTime"] == DBNull.Value ? null : (TimeSpan)dr["StartTime"];
                model.EndTime = dr["EndTime"] == DBNull.Value ? null : (TimeSpan)dr["EndTime"];
                model.MaxCapacity = dr["MaxCapacity"] == DBNull.Value ? null : Convert.ToInt32(dr["MaxCapacity"]);
                model.Amount = dr["Amount"] == DBNull.Value ? null : Convert.ToDecimal(dr["Amount"]);
                model.IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Update(ClassMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassQueries.Update, con);

            cmd.Parameters.AddWithValue("@ClassId", model.ClassId);
            cmd.Parameters.AddWithValue("@ClassName", model.ClassName);
            cmd.Parameters.AddWithValue("@TrainerId", (object?)model.TrainerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Schedule", (object?)model.Schedule ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartTime", (object?)model.StartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndTime", (object?)model.EndTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxCapacity", (object?)model.MaxCapacity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(ClassQueries.Delete, con);
            cmd.Parameters.AddWithValue("@ClassId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

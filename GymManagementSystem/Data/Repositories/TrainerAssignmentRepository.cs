using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class TrainerAssignmentRepository
    {
        private readonly DbHelper _db;

        public TrainerAssignmentRepository(DbHelper db)
        {
            _db = db;
        }

        public List<TrainerAssignmentModel> GetAll(
            string? search,
            bool? status)
        {
            List<TrainerAssignmentModel> list = new();

            using (SqlConnection con = _db.GetConnection())
            {
                con.Open();

                string query = @"
SELECT
    TA.AssignmentId,
    TA.MemberId,
    M.MemberName,
    TA.TrainerId,
    S.StaffName AS TrainerName,
    TA.StartDate,
    TA.EndDate,
    TA.Remarks,
    TA.IsActive,
    TA.CreatedDate,
    TA.CreatedBy
FROM TrainerAssignments TA
INNER JOIN MemberMasters M
ON TA.MemberId=M.MemberId
INNER JOIN StaffMasters S
ON TA.TrainerId=S.StaffId
WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += @"
AND
(
    M.MemberName LIKE @Search
    OR
    S.StaffName LIKE @Search
)";
                }

                if (status != null)
                {
                    query += @"
AND
TA.IsActive=@Status";
                }

                query += @"
ORDER BY TA.AssignmentId DESC";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");
                }

                if (status != null)
                {
                    cmd.Parameters.AddWithValue(
                        "@Status",
                        status);
                }

                SqlDataReader dr =
                    cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new TrainerAssignmentModel
                    {
                        AssignmentId =
                            Convert.ToInt32(dr["AssignmentId"]),

                        MemberId =
                            Convert.ToInt32(dr["MemberId"]),

                        MemberName =
                            dr["MemberName"].ToString()!,

                        TrainerId =
                            Convert.ToInt32(dr["TrainerId"]),

                        TrainerName =
                            dr["TrainerName"].ToString()!,

                        StartDate =
                            Convert.ToDateTime(dr["StartDate"]),

                        EndDate =
                            dr["EndDate"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(dr["EndDate"]),

                        Remarks =
                            dr["Remarks"]?.ToString() ?? "",

                        IsActive =
                            Convert.ToBoolean(dr["IsActive"]),

                        CreatedDate =
                            Convert.ToDateTime(dr["CreatedDate"]),

                        CreatedBy =
                            dr["CreatedBy"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(dr["CreatedBy"])
                    });
                }
            }

            return list;
        }

        public TrainerAssignmentModel GetById(int id)
        {
            TrainerAssignmentModel model = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    TrainerAssignmentQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue(
                "@AssignmentId",
                id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                model.AssignmentId =
                    Convert.ToInt32(dr["AssignmentId"]);

                model.MemberId =
                    Convert.ToInt32(dr["MemberId"]);

                model.TrainerId =
                    Convert.ToInt32(dr["TrainerId"]);

                model.StartDate =
                    Convert.ToDateTime(dr["StartDate"]);

                model.EndDate =
                    dr["EndDate"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(dr["EndDate"]);

                model.Remarks =
                    dr["Remarks"]?.ToString() ?? "";

                model.IsActive =
                    Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Insert(TrainerAssignmentModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    TrainerAssignmentQueries.Insert,
                    con);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                model.MemberId);

            cmd.Parameters.AddWithValue(
                "@TrainerId",
                model.TrainerId);

            cmd.Parameters.AddWithValue(
                "@StartDate",
                model.StartDate);

            cmd.Parameters.AddWithValue(
                "@EndDate",
                (object?)model.EndDate ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Remarks",
                model.Remarks ?? "");

            cmd.Parameters.AddWithValue(
                "@IsActive",
                model.IsActive);

            cmd.Parameters.AddWithValue(
                "@CreatedBy",
                (object?)model.CreatedBy ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Update(TrainerAssignmentModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    TrainerAssignmentQueries.Update,
                    con);

            cmd.Parameters.AddWithValue(
                "@AssignmentId",
                model.AssignmentId);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                model.MemberId);

            cmd.Parameters.AddWithValue(
                "@TrainerId",
                model.TrainerId);

            cmd.Parameters.AddWithValue(
                "@StartDate",
                model.StartDate);

            cmd.Parameters.AddWithValue(
                "@EndDate",
                (object?)model.EndDate ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Remarks",
                model.Remarks ?? "");

            cmd.Parameters.AddWithValue(
                "@IsActive",
                model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    TrainerAssignmentQueries.Delete,
                    con);

            cmd.Parameters.AddWithValue(
                "@AssignmentId",
                id);

            cmd.ExecuteNonQuery();
        }
    }
}
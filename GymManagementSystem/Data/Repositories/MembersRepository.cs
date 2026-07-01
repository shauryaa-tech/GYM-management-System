using GymManagement.Data.Queries;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class MembersRepository
    {
        private readonly DbHelper _db;

        public MembersRepository(DbHelper db)
        {
            _db = db;
        }

        public List<MemberMaster> GetAll(
            string? search,
            string? trainer,
            string? plan,
            string? status)
        {
            List<MemberMaster> list = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            string query =
                MembersQueries.GetAll;

            query += " WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += @"
AND
(
    M.MemberName LIKE @Search
    OR
    M.MemberCode LIKE @Search
    OR
    M.MobileNo LIKE @Search
)";
            }

            if (!string.IsNullOrWhiteSpace(trainer))
            {
                query += @"
AND
S.StaffName=@Trainer";
            }

            if (!string.IsNullOrWhiteSpace(plan))
            {
                query += @"
AND
MP.PlanName=@Plan";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query += @"
AND
M.Status=@Status";
            }

            query += @"
ORDER BY
M.MemberId DESC";

            SqlCommand cmd =
                new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(trainer))
            {
                cmd.Parameters.AddWithValue(
                    "@Trainer",
                    trainer);
            }

            if (!string.IsNullOrWhiteSpace(plan))
            {
                cmd.Parameters.AddWithValue(
                    "@Plan",
                    plan);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                cmd.Parameters.AddWithValue(
                    "@Status",
                    status);
            }

            SqlDataReader dr =
                cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new MemberMaster
                {
                    MemberId =
                        Convert.ToInt32(
                            dr["MemberId"]),

                    MemberCode =
                        dr["MemberCode"].ToString()!,

                    MemberName =
                        dr["MemberName"].ToString()!,

                    MobileNo =
                        dr["MobileNo"].ToString()!,

                    Status =
                        dr["Status"].ToString()!,

                    JoinDate =
                        Convert.ToDateTime(
                            dr["JoinDate"]),

                    TrainerName =
                        dr["TrainerName"]?.ToString(),

                    PlanName =
                        dr["PlanName"]?.ToString()
                });
            }

            return list;
        }

        public List<StaffMaster> GetTrainerList()
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
ON S.RoleId=R.RoleId
WHERE
    R.RoleName='Trainer'
AND
    S.IsActive=1
ORDER BY
    S.StaffName", con);

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

        public List<MembershipPlanMaster> GetPlanList()
        {
            List<MembershipPlanMaster> list = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(@"
SELECT
    PlanId,
    PlanName
FROM MembershipPlanMasters
WHERE IsActive=1
ORDER BY PlanName", con);

            SqlDataReader dr =
                cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new MembershipPlanMaster
                {
                    PlanId =
                        Convert.ToInt32(dr["PlanId"]),

                    PlanName =
                        dr["PlanName"].ToString()!
                });
            }

            return list;
        }

        public MemberMaster GetById(int id)
        {
            MemberMaster member = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MembersQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                member.MemberId =
                    Convert.ToInt32(dr["MemberId"]);

                member.MemberCode =
                    dr["MemberCode"].ToString()!;

                member.MemberName =
                    dr["MemberName"].ToString()!;

                member.MobileNo =
                    dr["MobileNo"].ToString()!;

                member.Email =
                    dr["Email"]?.ToString();

                member.Gender =
                    dr["Gender"]?.ToString();

                member.Address =
                    dr["Address"]?.ToString();

                member.Status =
                    dr["Status"].ToString()!;

                member.JoinDate =
                    Convert.ToDateTime(dr["JoinDate"]);

                member.Height =
                    dr["Height"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(dr["Height"]);

                member.Weight =
                    dr["Weight"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(dr["Weight"]);

                member.TrainerName =
                    dr["TrainerName"]?.ToString();

                member.PlanName =
                    dr["PlanName"]?.ToString();

                member.MedicalNotes =
                    dr["MedicalNotes"]?.ToString();

                member.EmergencyContact =
                    dr["EmergencyContact"]?.ToString();
            }

            return member;
        }
    }
}
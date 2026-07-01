using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class MemberRepository
    {
        private readonly DbHelper _db;

        public MemberRepository(DbHelper db)
        {
            _db = db;
        }

        private string GenerateMemberCode()
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    "SELECT ISNULL(MAX(MemberId),0)+1 FROM MemberMasters",
                    con);

            int nextId =
                Convert.ToInt32(
                    cmd.ExecuteScalar());

            return "M" + nextId.ToString("000");
        }

        public List<MemberMaster> GetAll(
            string? search,
            string? status)
        {
            List<MemberMaster> members = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            string query = @"
            SELECT
                M.*,
                S.StaffName AS TrainerName,
                P.PlanName
            FROM MemberMasters M

            LEFT JOIN StaffMasters S
            ON M.TrainerId = S.StaffId

            LEFT JOIN MembershipPlanMasters P
            ON M.PlanId = P.PlanId";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += @"
                AND
                (
                    M.MemberName LIKE @Search
                    OR
                    M.MobileNo LIKE @Search
                    OR
                    M.MemberCode LIKE @Search
                )";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query +=
                    " AND M.Status=@Status";
            }

            query +=
                " ORDER BY M.MemberId DESC";

            SqlCommand cmd =
                new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" + search + "%");
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
                members.Add(
                    new MemberMaster
                    {
                        MemberId =
                            Convert.ToInt32(
                                dr["MemberId"]),

                        MemberCode =
                            dr["MemberCode"]?.ToString() ?? "",

                        MemberName =
                            dr["MemberName"]?.ToString() ?? "",

                        MobileNo =
                            dr["MobileNo"]?.ToString() ?? "",

                        Email =
                            dr["Email"]?.ToString(),

                        Gender =
                            dr["Gender"]?.ToString(),

                        Address =
                            dr["Address"]?.ToString(),

                        Status =
                            dr["Status"]?.ToString() ?? "",

                        TrainerName =
                            dr["TrainerName"]?.ToString(),

                        PlanName =
                            dr["PlanName"]?.ToString(),

                        PlanStartDate =
                            Convert.ToDateTime(
                                dr["PlanStartDate"]),

                        PlanEndDate =
                            Convert.ToDateTime(
                                dr["PlanEndDate"]),
                    });
            }

            return members;
        }
        public int Insert(MemberMaster member)
        {
            member.MemberCode =
                GenerateMemberCode();

            if (member.PlanStartDate == DateTime.MinValue)
            {
                member.PlanStartDate = member.JoinDate;
            }

            if (member.PlanEndDate == DateTime.MinValue)
            {
                member.PlanEndDate =
                    member.JoinDate.AddMonths(1);
            }

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MemberQueries.Insert + "; SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    con);

            cmd.Parameters.AddWithValue(
                "@MemberCode",
                member.MemberCode);

            cmd.Parameters.AddWithValue(
                "@MemberName",
                member.MemberName);

            cmd.Parameters.AddWithValue(
                "@MobileNo",
                member.MobileNo);

            cmd.Parameters.AddWithValue(
                "@AlternateMobile",
                (object?)member.AlternateMobile ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Email",
                (object?)member.Email ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Gender",
                (object?)member.Gender ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@DateOfBirth",
                (object?)member.DateOfBirth ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@BloodGroup",
                (object?)member.BloodGroup ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Address",
                (object?)member.Address ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@City",
                (object?)member.City ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@State",
                (object?)member.State ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Pincode",
                (object?)member.Pincode ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@EmergencyContact",
                (object?)member.EmergencyContact ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@EmergencyContactName",
                (object?)member.EmergencyContactName ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@TrainerId",
                member.TrainerId);

            cmd.Parameters.AddWithValue(
                "@PlanId",
                member.PlanId);

            cmd.Parameters.AddWithValue(
                "@JoinDate",
                member.JoinDate);

            cmd.Parameters.AddWithValue(
                "@PlanStartDate",
                member.PlanStartDate);

            cmd.Parameters.AddWithValue(
                "@PlanEndDate",
                member.PlanEndDate);

            cmd.Parameters.AddWithValue(
                "@Height",
                member.Height);

            cmd.Parameters.AddWithValue(
                "@Weight",
                member.Weight);

            cmd.Parameters.AddWithValue(
                "@Status",
                member.Status);

            cmd.Parameters.AddWithValue(
                "@MedicalNotes",
                (object?)member.MedicalNotes ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Remarks",
                (object?)member.Remarks ?? DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public MemberMaster GetById(int id)
        {
            MemberMaster member = new();

            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MemberQueries.GetById,
                    con);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                id);

            SqlDataReader dr =
                cmd.ExecuteReader();

            if (dr.Read())
            {
                member.MemberId =
                    Convert.ToInt32(
                        dr["MemberId"]);

                member.MemberCode =
                    dr["MemberCode"]?.ToString() ?? "";

                member.MemberName =
                    dr["MemberName"]?.ToString() ?? "";

                member.MobileNo =
                    dr["MobileNo"]?.ToString() ?? "";

                member.Email =
                    dr["Email"]?.ToString();

                member.Gender =
                    dr["Gender"]?.ToString();

                member.Address =
                    dr["Address"]?.ToString();

                member.Status =
                    dr["Status"]?.ToString() ?? "";

                member.PlanId =
                    dr["PlanId"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(dr["PlanId"]);

                member.TrainerId =
                    dr["TrainerId"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(dr["TrainerId"]);

                member.PlanStartDate =
                    Convert.ToDateTime(
                        dr["PlanStartDate"]);

                member.PlanEndDate =
                    Convert.ToDateTime(
                        dr["PlanEndDate"]);

                member.JoinDate =
                    Convert.ToDateTime(
                        dr["JoinDate"]);

                member.Height =
    dr["Height"] == DBNull.Value
    ? 0
    : Convert.ToDecimal(dr["Height"]);

                member.Weight =
                    dr["Weight"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(dr["Weight"]);
            }

            return member;
        }

        public void Update(MemberMaster member)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MemberQueries.Update,
                    con);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                member.MemberId);

            cmd.Parameters.AddWithValue(
                "@MemberName",
                member.MemberName);

            cmd.Parameters.AddWithValue(
                "@MobileNo",
                member.MobileNo);

            cmd.Parameters.AddWithValue(
                "@AlternateMobile",
                (object?)member.AlternateMobile ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Email",
                (object?)member.Email ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Gender",
                (object?)member.Gender ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@DateOfBirth",
                (object?)member.DateOfBirth ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@BloodGroup",
                (object?)member.BloodGroup ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Address",
                (object?)member.Address ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@City",
                (object?)member.City ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@State",
                (object?)member.State ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Pincode",
                (object?)member.Pincode ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@EmergencyContact",
                (object?)member.EmergencyContact ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@EmergencyContactName",
                (object?)member.EmergencyContactName ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@TrainerId",
                member.TrainerId);

            cmd.Parameters.AddWithValue(
                "@PlanId",
                member.PlanId);

            cmd.Parameters.AddWithValue(
                "@JoinDate",
                member.JoinDate);

            cmd.Parameters.AddWithValue(
                "@PlanStartDate",
                member.PlanStartDate);

            cmd.Parameters.AddWithValue(
                "@PlanEndDate",
                member.PlanEndDate);

            cmd.Parameters.AddWithValue(
                "@Height",
                member.Height);

            cmd.Parameters.AddWithValue(
                "@Weight",
                member.Weight);

            cmd.Parameters.AddWithValue(
                "@Status",
                member.Status);

            cmd.Parameters.AddWithValue(
                "@MedicalNotes",
                (object?)member.MedicalNotes ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@Remarks",
                (object?)member.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            SqlCommand cmd =
                new SqlCommand(
                    MemberQueries.Delete,
                    con);

            cmd.Parameters.AddWithValue(
                "@MemberId",
                id);

            cmd.ExecuteNonQuery();
        }

        public List<MemberMaster> GetActiveMembers()
        {
            List<MemberMaster> list = new();

            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(@"
        SELECT
            MemberId,
            MemberName
        FROM MemberMasters
        WHERE Status='Active'
        ORDER BY MemberName", con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new MemberMaster
                {
                    MemberId =
                        Convert.ToInt32(dr["MemberId"]),

                    MemberName =
                        dr["MemberName"].ToString()!
                });
            }

            return list;
        }

        
    }
}
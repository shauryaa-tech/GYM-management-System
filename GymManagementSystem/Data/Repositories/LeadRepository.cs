using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class LeadRepository
    {
        private readonly DbHelper _db;

        public LeadRepository(DbHelper db)
        {
            _db = db;
        }

        private string GenerateLeadCode()
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(LeadId),0)+1 FROM Leads", con);
            int nextId = Convert.ToInt32(cmd.ExecuteScalar());
            return "LD" + nextId.ToString("000");
        }

        public List<Lead> GetAll(
            string? search,
            string? status,
            int? sourceId,
            int? assignedTo)
        {
            List<Lead> list = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = @"
SELECT
L.*,
LS.SourceName AS LeadSourceName,
S.StaffName AS AssignedStaffName
FROM Leads L
LEFT JOIN LeadSourceMasters LS ON L.LeadSourceId=LS.LeadSourceId
LEFT JOIN StaffMasters S ON L.AssignedTo=S.StaffId
WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += @"
AND (L.LeadName LIKE @Search OR L.MobileNo LIKE @Search OR L.Email LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(status))
                query += " AND L.Status=@Status";

            if (sourceId != null)
                query += " AND L.LeadSourceId=@LeadSourceId";

            if (assignedTo != null)
                query += " AND L.AssignedTo=@AssignedTo";

            query += " ORDER BY L.LeadId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@Status", status);

            if (sourceId != null)
                cmd.Parameters.AddWithValue("@LeadSourceId", sourceId);

            if (assignedTo != null)
                cmd.Parameters.AddWithValue("@AssignedTo", assignedTo);

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(MapLead(dr));

            return list;
        }

        public Lead GetById(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(LeadQueries.GetById, con);
            cmd.Parameters.AddWithValue("@LeadId", id);
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read() ? MapLead(dr) : new Lead();
        }

        public int Insert(Lead model)
        {
            model.LeadCode = GenerateLeadCode();
            MergeExtraFieldsIntoRemarks(model);

            try
            {
                return ExecuteInsert(LeadQueries.Insert, model, extended: true);
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase))
            {
                return ExecuteInsert(LeadQueries.InsertBasic, model, extended: false);
            }
        }

        public void Update(Lead model)
        {
            MergeExtraFieldsIntoRemarks(model);

            try
            {
                ExecuteUpdate(LeadQueries.Update, model, extended: true);
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteUpdate(LeadQueries.UpdateBasic, model, extended: false);
            }
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(LeadQueries.Delete, con);
            cmd.Parameters.AddWithValue("@LeadId", id);
            cmd.ExecuteNonQuery();
        }

        public string? GetLatestInterestedInByMobile(string mobileNo)
        {
            if (string.IsNullOrWhiteSpace(mobileNo))
                return null;

            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(LeadQueries.GetLatestInterestedInByMobile, con);
            cmd.Parameters.AddWithValue("@MobileNo", mobileNo.Trim());
            var result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        private int ExecuteInsert(string sql, Lead model, bool extended)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(sql + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
            BindInsertParams(cmd, model, extended);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void ExecuteUpdate(string sql, Lead model, bool extended)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@LeadId", model.LeadId);
            BindUpdateParams(cmd, model, extended);
            cmd.ExecuteNonQuery();
        }

        private static void BindInsertParams(SqlCommand cmd, Lead model, bool extended)
        {
            if (extended)
            {
                cmd.Parameters.AddWithValue("@LeadCode", model.LeadCode);
                cmd.Parameters.AddWithValue("@AlternateMobile", (object?)model.AlternateMobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object?)model.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Budget", (object?)model.Budget ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsConverted", model.IsConverted);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
            }

            cmd.Parameters.AddWithValue("@LeadName", model.LeadName);
            cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
            cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InterestedIn", (object?)model.InterestedIn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeadSourceId", model.LeadSourceId > 0 ? model.LeadSourceId : DBNull.Value);
            cmd.Parameters.AddWithValue("@AssignedTo", (object?)model.AssignedTo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(model.Status) ? "New" : model.Status);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FollowUpDate", (object?)model.FollowUpDate ?? DBNull.Value);
        }

        private static void BindUpdateParams(SqlCommand cmd, Lead model, bool extended)
        {
            if (extended)
            {
                cmd.Parameters.AddWithValue("@AlternateMobile", (object?)model.AlternateMobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object?)model.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Budget", (object?)model.Budget ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsConverted", model.IsConverted);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
            }

            cmd.Parameters.AddWithValue("@LeadName", model.LeadName);
            cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
            cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InterestedIn", (object?)model.InterestedIn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeadSourceId", model.LeadSourceId > 0 ? model.LeadSourceId : DBNull.Value);
            cmd.Parameters.AddWithValue("@AssignedTo", (object?)model.AssignedTo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", model.Status);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FollowUpDate", (object?)model.FollowUpDate ?? DBNull.Value);
        }

        private static void MergeExtraFieldsIntoRemarks(Lead model)
        {
            if (string.IsNullOrWhiteSpace(model.Gender))
                return;

            var tag = $"Gender: {model.Gender}";
            if (model.Remarks?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true)
                return;

            model.Remarks = string.IsNullOrWhiteSpace(model.Remarks)
                ? tag
                : $"{model.Remarks} | {tag}";
        }

        private static Lead MapLead(SqlDataReader dr)
        {
            var status = dr["Status"]?.ToString() ?? "New";
            int leadId = Convert.ToInt32(dr["LeadId"]);

            var lead = new Lead
            {
                LeadId = leadId,
                LeadCode = HasColumn(dr, "LeadCode") ? dr["LeadCode"]?.ToString() ?? "" : $"LD{leadId:000}",
                LeadName = dr["LeadName"]?.ToString() ?? "",
                MobileNo = dr["MobileNo"]?.ToString() ?? "",
                AlternateMobile = HasColumn(dr, "AlternateMobile") ? dr["AlternateMobile"]?.ToString() : null,
                Email = dr["Email"]?.ToString(),
                Gender = HasColumn(dr, "Gender") ? dr["Gender"]?.ToString() : null,
                Address = HasColumn(dr, "Address") ? dr["Address"]?.ToString() : null,
                InterestedIn = dr["InterestedIn"]?.ToString(),
                LeadSourceId = dr["LeadSourceId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["LeadSourceId"]),
                LeadSourceName = HasColumn(dr, "LeadSourceName") ? dr["LeadSourceName"]?.ToString() : null,
                AssignedTo = dr["AssignedTo"] == DBNull.Value ? null : Convert.ToInt32(dr["AssignedTo"]),
                AssignedStaffName = HasColumn(dr, "AssignedStaffName") ? dr["AssignedStaffName"]?.ToString() : null,
                Status = status,
                Budget = HasColumn(dr, "Budget") && dr["Budget"] != DBNull.Value ? Convert.ToDecimal(dr["Budget"]) : null,
                Remarks = dr["Remarks"]?.ToString(),
                FollowUpDate = dr["FollowUpDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["FollowUpDate"]),
                IsConverted = HasColumn(dr, "IsConverted")
                    ? Convert.ToBoolean(dr["IsConverted"])
                    : string.Equals(status, "Converted", StringComparison.OrdinalIgnoreCase),
                IsActive = HasColumn(dr, "IsActive")
                    ? Convert.ToBoolean(dr["IsActive"])
                    : status is not "Converted" and not "Lost",
                CreatedDate = dr["CreatedDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(dr["CreatedDate"]),
                ModifiedDate = HasColumn(dr, "ModifiedDate") && dr["ModifiedDate"] != DBNull.Value
                    ? Convert.ToDateTime(dr["ModifiedDate"])
                    : null
            };

            return lead;
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

using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class EquipmentRepository
    {
        private readonly DbHelper _db;

        public EquipmentRepository(DbHelper db)
        {
            _db = db;
        }

        public List<EquipmentMaster> GetAll(string? search, string? conditionStatus)
        {
            List<EquipmentMaster> equipments = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = EquipmentQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (EquipmentName LIKE @Search OR Category LIKE @Search OR Location LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(conditionStatus))
            {
                query += " AND ConditionStatus=@ConditionStatus";
            }

            query += " ORDER BY EquipmentId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(conditionStatus))
            {
                cmd.Parameters.AddWithValue("@ConditionStatus", conditionStatus);
            }

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                equipments.Add(new EquipmentMaster
                {
                    EquipmentId = Convert.ToInt32(dr["EquipmentId"]),
                    EquipmentName = dr["EquipmentName"]?.ToString() ?? "",
                    Category = dr["Category"]?.ToString(),
                    PurchaseDate = dr["PurchaseDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["PurchaseDate"]),
                    PurchasePrice = dr["PurchasePrice"] == DBNull.Value ? null : Convert.ToDecimal(dr["PurchasePrice"]),
                    Quantity = dr["Quantity"] == DBNull.Value ? null : Convert.ToInt32(dr["Quantity"]),
                    ConditionStatus = dr["ConditionStatus"]?.ToString(),
                    Location = dr["Location"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString(),
                    IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                });
            }

            return equipments;
        }

        public void Insert(EquipmentMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentQueries.Insert, con);

            cmd.Parameters.AddWithValue("@EquipmentName", model.EquipmentName);
            cmd.Parameters.AddWithValue("@Category", (object?)model.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchaseDate", (object?)model.PurchaseDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchasePrice", (object?)model.PurchasePrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Quantity", (object?)model.Quantity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConditionStatus", (object?)model.ConditionStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Location", (object?)model.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public EquipmentMaster GetById(int id)
        {
            EquipmentMaster model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentQueries.GetById, con);
            cmd.Parameters.AddWithValue("@EquipmentId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.EquipmentId = Convert.ToInt32(dr["EquipmentId"]);
                model.EquipmentName = dr["EquipmentName"]?.ToString() ?? "";
                model.Category = dr["Category"]?.ToString();
                model.PurchaseDate = dr["PurchaseDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["PurchaseDate"]);
                model.PurchasePrice = dr["PurchasePrice"] == DBNull.Value ? null : Convert.ToDecimal(dr["PurchasePrice"]);
                model.Quantity = dr["Quantity"] == DBNull.Value ? null : Convert.ToInt32(dr["Quantity"]);
                model.ConditionStatus = dr["ConditionStatus"]?.ToString();
                model.Location = dr["Location"]?.ToString();
                model.Remarks = dr["Remarks"]?.ToString();
                model.IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"]);
            }

            return model;
        }

        public void Update(EquipmentMaster model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentQueries.Update, con);

            cmd.Parameters.AddWithValue("@EquipmentId", model.EquipmentId);
            cmd.Parameters.AddWithValue("@EquipmentName", model.EquipmentName);
            cmd.Parameters.AddWithValue("@Category", (object?)model.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchaseDate", (object?)model.PurchaseDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchasePrice", (object?)model.PurchasePrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Quantity", (object?)model.Quantity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConditionStatus", (object?)model.ConditionStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Location", (object?)model.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentQueries.Delete, con);
            cmd.Parameters.AddWithValue("@EquipmentId", id);

            cmd.ExecuteNonQuery();
        }
    }
}

using GymManagement.Models;
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class EquipmentMaintenanceRepository
    {
        private readonly DbHelper _db;

        public EquipmentMaintenanceRepository(DbHelper db)
        {
            _db = db;
        }

        public List<EquipmentMaintenance> GetAll(string? search, string? equipmentId, string? status, string? fromDate, string? toDate)
        {
            List<EquipmentMaintenance> list = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = EquipmentMaintenanceQueries.GetAll;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (E.EquipmentName LIKE @Search OR EM.VendorName LIKE @Search OR EM.MaintenanceType LIKE @Search)";
            }

            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                query += " AND EM.EquipmentId=@EquipmentId";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query += " AND EM.Status=@Status";
            }

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                query += " AND EM.MaintenanceDate >= @FromDate";
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                query += " AND EM.MaintenanceDate <= @ToDate";
            }

            query += " ORDER BY EM.MaintenanceDate DESC, EM.MaintenanceId DESC";

            SqlCommand cmd = new SqlCommand(query, con);

            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            }

            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
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
                list.Add(MapEquipmentMaintenance(dr));
            }

            return list;
        }

        public void Insert(EquipmentMaintenance model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentMaintenanceQueries.Insert, con);

            cmd.Parameters.AddWithValue("@EquipmentId", model.EquipmentId);
            cmd.Parameters.AddWithValue("@MaintenanceDate", model.MaintenanceDate);
            cmd.Parameters.AddWithValue("@MaintenanceType", model.MaintenanceType);
            cmd.Parameters.AddWithValue("@Cost", model.Cost);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VendorName", (object?)model.VendorName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NextDueDate", (object?)model.NextDueDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", model.Status);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public EquipmentMaintenance GetById(int id)
        {
            EquipmentMaintenance model = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentMaintenanceQueries.GetById, con);
            cmd.Parameters.AddWithValue("@MaintenanceId", id);

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model = MapEquipmentMaintenance(dr);
            }

            return model;
        }

        public void Update(EquipmentMaintenance model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentMaintenanceQueries.Update, con);

            cmd.Parameters.AddWithValue("@MaintenanceId", model.MaintenanceId);
            cmd.Parameters.AddWithValue("@EquipmentId", model.EquipmentId);
            cmd.Parameters.AddWithValue("@MaintenanceDate", model.MaintenanceDate);
            cmd.Parameters.AddWithValue("@MaintenanceType", model.MaintenanceType);
            cmd.Parameters.AddWithValue("@Cost", model.Cost);
            cmd.Parameters.AddWithValue("@PaymentMode", (object?)model.PaymentMode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VendorName", (object?)model.VendorName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NextDueDate", (object?)model.NextDueDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", model.Status);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentMaintenanceQueries.Delete, con);
            cmd.Parameters.AddWithValue("@MaintenanceId", id);

            cmd.ExecuteNonQuery();
        }

        public List<EquipmentMaster> GetActiveEquipment()
        {
            List<EquipmentMaster> equipment = new();

            using SqlConnection con = _db.GetConnection();
            con.Open();

            SqlCommand cmd = new SqlCommand(EquipmentMaintenanceQueries.GetActiveEquipment, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                equipment.Add(new EquipmentMaster
                {
                    EquipmentId = Convert.ToInt32(dr["EquipmentId"]),
                    EquipmentName = dr["EquipmentName"]?.ToString() ?? "",
                    Category = dr["Category"]?.ToString(),
                    Location = dr["Location"]?.ToString()
                });
            }

            return equipment;
        }

        private static EquipmentMaintenance MapEquipmentMaintenance(SqlDataReader dr)
        {
            return new EquipmentMaintenance
            {
                MaintenanceId = Convert.ToInt32(dr["MaintenanceId"]),
                EquipmentId = Convert.ToInt32(dr["EquipmentId"]),
                MaintenanceDate = Convert.ToDateTime(dr["MaintenanceDate"]),
                MaintenanceType = dr["MaintenanceType"]?.ToString() ?? "",
                Cost = Convert.ToDecimal(dr["Cost"]),
                PaymentMode = dr["PaymentMode"]?.ToString(),
                VendorName = dr["VendorName"]?.ToString(),
                NextDueDate = dr["NextDueDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["NextDueDate"]),
                Status = dr["Status"]?.ToString() ?? "Completed",
                Remarks = dr["Remarks"]?.ToString(),
                EquipmentName = dr["EquipmentName"]?.ToString(),
                Category = dr["Category"]?.ToString(),
                Location = dr["Location"]?.ToString()
            };
        }
    }
}

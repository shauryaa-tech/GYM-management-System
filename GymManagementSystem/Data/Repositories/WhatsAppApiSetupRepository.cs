using GymManagement.Data.Queries;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class WhatsAppApiSetupRepository
    {
        private readonly DbHelper _db;

        public WhatsAppApiSetupRepository(DbHelper db)
        {
            _db = db;
        }

        public WhatsAppApiSettingsEntity? Get()
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();
                SqlCommand cmd = new SqlCommand(WhatsAppApiSetupQueries.Get, con);
                using SqlDataReader dr = cmd.ExecuteReader();
                return dr.Read() ? Map(dr) : null;
            }
            catch
            {
                return null;
            }
        }

        public void Save(WhatsAppApiSettingsEntity entity, int? userId)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(WhatsAppApiSetupQueries.Upsert, con);
            cmd.Parameters.AddWithValue("@IsEnabled", entity.IsEnabled);
            cmd.Parameters.AddWithValue("@ApiProvider", entity.ApiProvider ?? "SmartPing");
            cmd.Parameters.AddWithValue("@ApiBaseUrl", (object?)entity.ApiBaseUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumberId", (object?)entity.PhoneNumberId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BusinessPhone", (object?)entity.BusinessPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WabaId", (object?)entity.WabaId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AppId", (object?)entity.AppId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccessToken", (object?)entity.AccessToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VerifyToken", (object?)entity.VerifyToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GraphApiVersion", entity.GraphApiVersion ?? "v21.0");
            cmd.Parameters.AddWithValue("@WelcomeMessage", (object?)entity.WelcomeMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ModifiedBy", (object?)userId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static WhatsAppApiSettingsEntity Map(SqlDataReader dr) => new()
        {
            Id = Convert.ToInt32(dr["Id"]),
            IsEnabled = Convert.ToBoolean(dr["IsEnabled"]),
            ApiProvider = HasColumn(dr, "ApiProvider")
                ? dr["ApiProvider"]?.ToString() ?? "SmartPing"
                : "SmartPing",
            ApiBaseUrl = HasColumn(dr, "ApiBaseUrl") ? dr["ApiBaseUrl"]?.ToString() : null,
            PhoneNumberId = dr["PhoneNumberId"]?.ToString(),
            BusinessPhone = dr["BusinessPhone"]?.ToString(),
            WabaId = dr["WabaId"]?.ToString(),
            AppId = dr["AppId"]?.ToString(),
            AccessToken = dr["AccessToken"]?.ToString(),
            VerifyToken = dr["VerifyToken"]?.ToString(),
            GraphApiVersion = dr["GraphApiVersion"]?.ToString() ?? "v21.0",
            WelcomeMessage = dr["WelcomeMessage"]?.ToString(),
            ModifiedBy = dr["ModifiedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["ModifiedBy"]),
            ModifiedDate = dr["ModifiedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["ModifiedDate"])
        };

        private static bool HasColumn(SqlDataReader dr, string column)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(dr.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}

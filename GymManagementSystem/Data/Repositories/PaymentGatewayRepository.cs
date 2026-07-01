using GymManagement.Data.Queries;
using GymManagement.Data.Repositories.Interfaces;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class PaymentGatewayRepository : IPaymentGatewayRepository
    {
        private readonly DbHelper _db;

        public PaymentGatewayRepository(DbHelper db)
        {
            _db = db;
        }

        public Task<List<PaymentGateway>> GetAllAsync(string? search, string? environment, string? status)
        {
            var list = new List<PaymentGateway>();
            var sql = PaymentGatewayQueries.BaseSelect;

            if (!string.IsNullOrWhiteSpace(search))
                sql += " AND (DisplayName LIKE @Search OR GatewayName LIKE @Search OR MID LIKE @Search)";

            if (!string.IsNullOrWhiteSpace(environment))
                sql += " AND Environment = @Environment";

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    sql += " AND IsActive = 1";
                else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    sql += " AND IsActive = 0";
            }

            sql += " ORDER BY IsDefault DESC, IsActive DESC, DisplayName";

            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(sql, con);

            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
            if (!string.IsNullOrWhiteSpace(environment))
                cmd.Parameters.AddWithValue("@Environment", environment);

            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(MapGateway(dr));

            return Task.FromResult(list);
        }

        public Task<PaymentGateway?> GetByIdAsync(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.GetById, con);
            cmd.Parameters.AddWithValue("@Id", id);
            using SqlDataReader dr = cmd.ExecuteReader();
            return Task.FromResult(dr.Read() ? MapGateway(dr) : null);
        }

        public Task<PaymentGateway?> GetDefaultActiveAsync()
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.GetDefaultActive, con);
            using SqlDataReader dr = cmd.ExecuteReader();
            return Task.FromResult(dr.Read() ? MapGateway(dr) : null);
        }

        public Task<PaymentGateway?> GetByGatewayNameAsync(string gatewayName)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.GetByGatewayName, con);
            cmd.Parameters.AddWithValue("@GatewayName", gatewayName);
            using SqlDataReader dr = cmd.ExecuteReader();
            return Task.FromResult(dr.Read() ? MapGateway(dr) : null);
        }

        public Task AddAsync(PaymentGateway gateway)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.Insert + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", con);
            BindGateway(cmd, gateway, includeId: false);
            gateway.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return Task.CompletedTask;
        }

        public Task UpdateAsync(PaymentGateway gateway)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.Update, con);
            BindGateway(cmd, gateway, includeId: true);
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.Delete, con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        public Task ClearDefaultAsync(int? exceptId = null)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            using SqlCommand cmd = new SqlCommand(PaymentGatewayQueries.ClearDefault, con);
            cmd.Parameters.AddWithValue("@ExceptId", (object?)exceptId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        private static PaymentGateway MapGateway(SqlDataReader dr) => new()
        {
            Id = Convert.ToInt32(dr["Id"]),
            GatewayName = dr["GatewayName"]?.ToString() ?? "",
            DisplayName = dr["DisplayName"]?.ToString() ?? "",
            MerchantId = dr["MerchantId"]?.ToString(),
            MerchantKey = dr["MerchantKey"]?.ToString(),
            MID = dr["MID"]?.ToString(),
            ChannelId = dr["ChannelId"]?.ToString(),
            Website = dr["Website"]?.ToString(),
            IndustryType = dr["IndustryType"]?.ToString(),
            CallbackUrl = dr["CallbackUrl"]?.ToString(),
            Environment = dr["Environment"]?.ToString() ?? "Sandbox",
            SandboxBaseUrl = dr["SandboxBaseUrl"]?.ToString(),
            ProductionBaseUrl = dr["ProductionBaseUrl"]?.ToString(),
            IsDefault = Convert.ToBoolean(dr["IsDefault"]),
            IsActive = Convert.ToBoolean(dr["IsActive"]),
            IsValidated = Convert.ToBoolean(dr["IsValidated"]),
            ValidationMessage = dr["ValidationMessage"]?.ToString(),
            LastValidatedOn = dr["LastValidatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["LastValidatedOn"]),
            CreatedBy = dr["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["CreatedBy"]),
            CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
            ModifiedBy = dr["ModifiedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["ModifiedBy"]),
            ModifiedDate = dr["ModifiedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["ModifiedDate"])
        };

        private static void BindGateway(SqlCommand cmd, PaymentGateway g, bool includeId)
        {
            if (includeId)
                cmd.Parameters.AddWithValue("@Id", g.Id);

            cmd.Parameters.AddWithValue("@GatewayName", g.GatewayName);
            cmd.Parameters.AddWithValue("@DisplayName", g.DisplayName);
            cmd.Parameters.AddWithValue("@MerchantId", (object?)g.MerchantId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MerchantKey", (object?)g.MerchantKey ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MID", (object?)g.MID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ChannelId", (object?)g.ChannelId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Website", (object?)g.Website ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IndustryType", (object?)g.IndustryType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CallbackUrl", (object?)g.CallbackUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Environment", g.Environment);
            cmd.Parameters.AddWithValue("@SandboxBaseUrl", (object?)g.SandboxBaseUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProductionBaseUrl", (object?)g.ProductionBaseUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsDefault", g.IsDefault);
            cmd.Parameters.AddWithValue("@IsActive", g.IsActive);
            cmd.Parameters.AddWithValue("@IsValidated", g.IsValidated);
            cmd.Parameters.AddWithValue("@ValidationMessage", (object?)g.ValidationMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastValidatedOn", (object?)g.LastValidatedOn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)g.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDate", g.CreatedDate);
            cmd.Parameters.AddWithValue("@ModifiedBy", (object?)g.ModifiedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ModifiedDate", (object?)g.ModifiedDate ?? DBNull.Value);
        }
    }
}

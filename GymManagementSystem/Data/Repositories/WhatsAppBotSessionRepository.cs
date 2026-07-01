using GymManagement.Data.Queries;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class WhatsAppBotSessionRepository
    {
        private readonly DbHelper _db;

        public WhatsAppBotSessionRepository(DbHelper db)
        {
            _db = db;
        }

        public int? GetLeadSourceId(string sourceName)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(WhatsAppBotQueries.GetLeadSourceIdByName, con);
            cmd.Parameters.AddWithValue("@SourceName", sourceName);
            var result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        public int CreateSession(int leadId, string phoneNumber)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(WhatsAppBotQueries.InsertSession, con);
            cmd.Parameters.AddWithValue("@LeadId", leadId);
            cmd.Parameters.AddWithValue("@PhoneNumber", NormalizePhone(phoneNumber));
            cmd.Parameters.AddWithValue("@CurrentStep", WhatsAppBotSteps.SelectTrainer);
            cmd.Parameters.AddWithValue("@PaymentToken", Guid.NewGuid().ToString("N"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public WhatsAppBotSession? GetActiveByPhone(string phoneNumber)
        {
            return QuerySingle(WhatsAppBotQueries.GetActiveByPhone, phoneNumber);
        }

        public WhatsAppBotSession? GetByPaymentToken(string token)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(WhatsAppBotQueries.GetByPaymentToken, con);
            cmd.Parameters.AddWithValue("@PaymentToken", token);
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read() ? Map(dr) : null;
        }

        public WhatsAppBotSession? GetByLeadId(int leadId)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(WhatsAppBotQueries.GetByLeadId, con);
            cmd.Parameters.AddWithValue("@LeadId", leadId);
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read() ? Map(dr) : null;
        }

        public void Update(WhatsAppBotSession session)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(WhatsAppBotQueries.UpdateSession, con);
            cmd.Parameters.AddWithValue("@SessionId", session.SessionId);
            cmd.Parameters.AddWithValue("@CurrentStep", session.CurrentStep);
            cmd.Parameters.AddWithValue("@SelectedTrainerId", (object?)session.SelectedTrainerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SelectedClassId", (object?)session.SelectedClassId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SelectedPlanId", (object?)session.SelectedPlanId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentToken", (object?)session.PaymentToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsCompleted", session.IsCompleted);
            cmd.ExecuteNonQuery();
        }

        public static string NormalizePhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 10)
                return "91" + digits;
            return digits;
        }

        private WhatsAppBotSession? QuerySingle(string sql, string phoneNumber)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@PhoneNumber", NormalizePhone(phoneNumber));
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read() ? Map(dr) : null;
        }

        private static WhatsAppBotSession Map(SqlDataReader dr) => new()
        {
            SessionId = Convert.ToInt32(dr["SessionId"]),
            LeadId = Convert.ToInt32(dr["LeadId"]),
            PhoneNumber = dr["PhoneNumber"]?.ToString() ?? string.Empty,
            CurrentStep = dr["CurrentStep"]?.ToString() ?? WhatsAppBotSteps.SelectTrainer,
            SelectedTrainerId = dr["SelectedTrainerId"] == DBNull.Value ? null : Convert.ToInt32(dr["SelectedTrainerId"]),
            SelectedClassId = dr["SelectedClassId"] == DBNull.Value ? null : Convert.ToInt32(dr["SelectedClassId"]),
            SelectedPlanId = dr["SelectedPlanId"] == DBNull.Value ? null : Convert.ToInt32(dr["SelectedPlanId"]),
            PaymentToken = dr["PaymentToken"]?.ToString(),
            IsCompleted = Convert.ToBoolean(dr["IsCompleted"]),
            CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
            UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedDate"]),
            LeadName = dr["LeadName"]?.ToString()
        };
    }
}

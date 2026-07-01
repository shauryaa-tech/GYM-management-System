using Microsoft.Data.SqlClient;

namespace GymManagement.Data
{
    /// <summary>
    /// Applies safe, idempotent column migrations on startup (COL_LENGTH checks).
    /// </summary>
    public class DatabaseSchemaUpdater
    {
        private readonly DbHelper _db;
        private readonly ILogger<DatabaseSchemaUpdater> _logger;

        public DatabaseSchemaUpdater(DbHelper db, ILogger<DatabaseSchemaUpdater> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void ApplyPendingMigrations()
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                ApplySalaryRuleMasterColumns(con);
                ApplySalaryProcessingColumns(con);
                ApplyStaffProfileColumns(con);
                ApplyPayrollPermission(con);
                ApplyWhatsAppApiColumns(con);

                _logger.LogInformation("Database schema migrations applied.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database schema update failed — run Database/SalaryRules_MissingColumns.sql manually.");
            }
        }

        private static void ApplySalaryRuleMasterColumns(SqlConnection con)
        {
            if (!TableExists(con, "SalaryRuleMasters"))
                return;

            EnsureColumn(con, "SalaryRuleMasters", "ShiftStartTime", "TIME NULL");
            EnsureColumn(con, "SalaryRuleMasters", "ShiftEndTime", "TIME NULL");
            EnsureColumn(con, "SalaryRuleMasters", "EarlyLeaveGraceMinutes", "INT NOT NULL CONSTRAINT DF_SalaryRule_EarlyLeaveGrace DEFAULT 0");
            EnsureColumn(con, "SalaryRuleMasters", "EnableSandwichRule", "BIT NOT NULL CONSTRAINT DF_SalaryRule_Sandwich DEFAULT 0");
            EnsureColumn(con, "SalaryRuleMasters", "WeeklyOffDays", "NVARCHAR(100) NULL");
            EnsureColumn(con, "SalaryRuleMasters", "LateCountsAsHalfDay", "BIT NOT NULL CONSTRAINT DF_SalaryRule_LateHalf DEFAULT 1");
            EnsureColumn(con, "SalaryRuleMasters", "EarlyLeaveCountsAsHalfDay", "BIT NOT NULL CONSTRAINT DF_SalaryRule_EarlyHalf DEFAULT 1");
        }

        private static void ApplySalaryProcessingColumns(SqlConnection con)
        {
            if (!TableExists(con, "SalaryProcessings"))
                return;

            EnsureColumn(con, "SalaryProcessings", "PresentDays", "INT NULL");
            EnsureColumn(con, "SalaryProcessings", "AbsentDays", "INT NULL");
            EnsureColumn(con, "SalaryProcessings", "LeaveDays", "INT NULL");
            EnsureColumn(con, "SalaryProcessings", "HalfDays", "INT NULL");
            EnsureColumn(con, "SalaryProcessings", "SalaryRuleId", "INT NULL");
        }

        private static void ApplyStaffProfileColumns(SqlConnection con)
        {
            if (!TableExists(con, "StaffMasters"))
                return;

            EnsureColumn(con, "StaffMasters", "StaffCode", "NVARCHAR(20) NULL");
            EnsureColumn(con, "StaffMasters", "BankName", "NVARCHAR(150) NULL");
            EnsureColumn(con, "StaffMasters", "BankAccountNo", "NVARCHAR(50) NULL");
            EnsureColumn(con, "StaffMasters", "IfscCode", "NVARCHAR(20) NULL");

            using SqlCommand backfill = new SqlCommand(@"
                UPDATE StaffMasters
                SET StaffCode = 'EMP' + RIGHT('000' + CAST(StaffId AS VARCHAR(10)), 3)
                WHERE StaffCode IS NULL OR LTRIM(RTRIM(StaffCode)) = ''", con);
            backfill.ExecuteNonQuery();
        }

        private static void ApplyPayrollPermission(SqlConnection con)
        {
            if (!TableExists(con, "PermissionMaster"))
                return;

            using (SqlCommand insert = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE ModuleName = 'Payroll')
                INSERT INTO PermissionMaster (ModuleName, DisplayName, SortOrder, IsActive)
                VALUES ('Payroll', 'Payroll', 88, 1)", con))
            {
                insert.ExecuteNonQuery();
            }

            using (SqlCommand admin = new SqlCommand(@"
                INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
                SELECT 1, P.PermissionId, 1, 1, 1, 1, 1
                FROM PermissionMaster P
                WHERE P.ModuleName = 'Payroll'
                AND NOT EXISTS (
                    SELECT 1 FROM RolePermission RP
                    WHERE RP.RoleId = 1 AND RP.PermissionId = P.PermissionId)", con))
            {
                admin.ExecuteNonQuery();
            }

            using (SqlCommand copy = new SqlCommand(@"
                INSERT INTO RolePermission (RoleId, PermissionId, CanView, CanAdd, CanEdit, CanDelete, CanExport)
                SELECT SP.RoleId, PP.PermissionId,
                       SP.CanView, SP.CanAdd, SP.CanEdit, SP.CanDelete, SP.CanExport
                FROM RolePermission SP
                INNER JOIN PermissionMaster PM ON PM.PermissionId = SP.PermissionId AND PM.ModuleName = 'SalaryProcessing'
                CROSS JOIN PermissionMaster PP
                WHERE PP.ModuleName = 'Payroll'
                AND NOT EXISTS (
                    SELECT 1 FROM RolePermission RP
                    WHERE RP.RoleId = SP.RoleId AND RP.PermissionId = PP.PermissionId)", con))
            {
                copy.ExecuteNonQuery();
            }
        }

        private static void ApplyWhatsAppApiColumns(SqlConnection con)
        {
            if (!TableExists(con, "WhatsAppApiSettings"))
                return;

            EnsureColumn(con, "WhatsAppApiSettings", "ApiProvider", "NVARCHAR(50) NOT NULL CONSTRAINT DF_WA_ApiProvider DEFAULT 'SmartPing'");
            EnsureColumn(con, "WhatsAppApiSettings", "ApiBaseUrl", "NVARCHAR(500) NULL");

            using SqlCommand backfill = new SqlCommand(@"
                UPDATE WhatsAppApiSettings
                SET ApiProvider = 'SmartPing'
                WHERE ApiProvider IS NULL OR LTRIM(RTRIM(ApiProvider)) = ''", con);
            backfill.ExecuteNonQuery();
        }

        private static bool TableExists(SqlConnection con, string tableName)
        {
            using SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Table", con);
            cmd.Parameters.AddWithValue("@Table", tableName);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static void EnsureColumn(SqlConnection con, string table, string column, string definition)
        {
            using SqlCommand check = new SqlCommand(
                "SELECT COL_LENGTH(@Table, @Column)", con);
            check.Parameters.AddWithValue("@Table", table);
            check.Parameters.AddWithValue("@Column", column);
            if (check.ExecuteScalar() != DBNull.Value)
                return;

            using SqlCommand alter = new SqlCommand(
                $"ALTER TABLE [{table}] ADD [{column}] {definition}", con);
            alter.ExecuteNonQuery();
        }
    }
}

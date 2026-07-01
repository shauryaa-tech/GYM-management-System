using GymManagement.Data.Queries;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class RolePermissionRepository
    {
        private readonly DbHelper _db;

        public RolePermissionRepository(DbHelper db)
        {
            _db = db;
        }

        public List<RolePermission> GetByRole(long roleId)
        {
            List<RolePermission> list = new();

            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(RolePermissionQueries.GetByRole, con);

            cmd.Parameters.AddWithValue("@RoleId", roleId);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new RolePermission
                {
                    PermissionId = Convert.ToInt32(dr["PermissionId"]),
                    ModuleName = dr["ModuleName"].ToString(),
                    DisplayName = dr["DisplayName"].ToString(),

                    CanView = Convert.ToBoolean(dr["CanView"]),
                    CanAdd = Convert.ToBoolean(dr["CanAdd"]),
                    CanEdit = Convert.ToBoolean(dr["CanEdit"]),
                    CanDelete = Convert.ToBoolean(dr["CanDelete"]),
                    CanExport = Convert.ToBoolean(dr["CanExport"])
                });
            }

            return list;
        }

        public void Save(long roleId, List<RolePermission> permissions)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlTransaction tran = con.BeginTransaction();

            try
            {
                SqlCommand deleteCmd = new SqlCommand(
                    RolePermissionQueries.Delete,
                    con,
                    tran);

                deleteCmd.Parameters.AddWithValue("@RoleId", roleId);

                deleteCmd.ExecuteNonQuery();

                foreach (var item in permissions)
                {
                    SqlCommand insertCmd = new SqlCommand(
                        RolePermissionQueries.Insert,
                        con,
                        tran);

                    insertCmd.Parameters.AddWithValue("@RoleId", roleId);
                    insertCmd.Parameters.AddWithValue("@PermissionId", item.PermissionId);
                    insertCmd.Parameters.AddWithValue("@CanView", item.CanView);
                    insertCmd.Parameters.AddWithValue("@CanAdd", item.CanAdd);
                    insertCmd.Parameters.AddWithValue("@CanEdit", item.CanEdit);
                    insertCmd.Parameters.AddWithValue("@CanDelete", item.CanDelete);
                    insertCmd.Parameters.AddWithValue("@CanExport", item.CanExport);

                    insertCmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public bool HasPermission(int roleId, string module, string permission)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            string column = permission switch
            {
                "View" => "CanView",
                "Add" => "CanAdd",
                "Edit" => "CanEdit",
                "Delete" => "CanDelete",
                "Export" => "CanExport",
                _ => "CanView"
            };

            string sql = $@"
                SELECT {column}
                FROM RolePermission RP
                INNER JOIN PermissionMaster P
                ON RP.PermissionId=P.PermissionId
                WHERE RP.RoleId=@RoleId
                AND P.ModuleName=@Module";

            SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@RoleId", roleId);
            cmd.Parameters.AddWithValue("@Module", module);

            object? result = cmd.ExecuteScalar();

            if (result == null)
                return false;

            return Convert.ToBoolean(result);
        }
    }
}
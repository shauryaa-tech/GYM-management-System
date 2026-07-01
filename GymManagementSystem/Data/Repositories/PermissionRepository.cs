using GymManagement.Data.Queries;
using GymManagement.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class PermissionRepository
    {
        private readonly DbHelper _db;

        public PermissionRepository(DbHelper db)
        {
            _db = db;
        }

        public List<PermissionMaster> GetAll()
        {
            List<PermissionMaster> list = new();

            using SqlConnection con = _db.GetConnection();

            con.Open();

            SqlCommand cmd = new SqlCommand(PermissionQueries.GetAll, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new PermissionMaster
                {
                    PermissionId = Convert.ToInt32(dr["PermissionId"]),
                    ModuleName = dr["ModuleName"].ToString() ?? "",
                    DisplayName = dr["DisplayName"].ToString() ?? "",
                    SortOrder = Convert.ToInt32(dr["SortOrder"]),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                });
            }

            return list;
        }
    }
}
using GymManagement.Data.Queries;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class NavbarSearchRepository
    {
        private readonly DbHelper _db;

        public NavbarSearchRepository(DbHelper db)
        {
            _db = db;
        }

        public List<NavbarSearchResult> Search(string query, int limitPerType = 5)
        {
            var results = new List<NavbarSearchResult>();
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return results;

            var pattern = $"%{query.Trim()}%";

            using SqlConnection con = _db.GetConnection();
            con.Open();

            TrySearch(con, NavbarSearchQueries.SearchMembers, pattern, reader =>
            {
                var id = Convert.ToInt32(reader["MemberId"]);
                results.Add(new NavbarSearchResult
                {
                    Type = "member",
                    Id = id,
                    Title = reader["MemberName"]?.ToString() ?? "",
                    Subtitle = reader["MemberCode"]?.ToString() ?? "",
                    Url = $"/Members/Details/{id}",
                    Icon = "fa-solid fa-user"
                });
            });

            TrySearch(con, NavbarSearchQueries.SearchClasses, pattern, reader =>
            {
                var id = Convert.ToInt32(reader["ClassId"]);
                results.Add(new NavbarSearchResult
                {
                    Type = "class",
                    Id = id,
                    Title = reader["ClassName"]?.ToString() ?? "",
                    Subtitle = reader["Schedule"]?.ToString() ?? "",
                    Url = "/ClassMaster/Index",
                    Icon = "fa-solid fa-users"
                });
            });

            TrySearch(con, NavbarSearchQueries.SearchProducts, pattern, reader =>
            {
                var id = Convert.ToInt32(reader["ProductId"]);
                results.Add(new NavbarSearchResult
                {
                    Type = "product",
                    Id = id,
                    Title = reader["ProductName"]?.ToString() ?? "",
                    Subtitle = reader["Category"]?.ToString() ?? "",
                    Url = "/Products/Index",
                    Icon = "fa-solid fa-box"
                });
            });

            TrySearch(con, NavbarSearchQueries.SearchLeads, pattern, reader =>
            {
                var id = Convert.ToInt32(reader["LeadId"]);
                results.Add(new NavbarSearchResult
                {
                    Type = "lead",
                    Id = id,
                    Title = reader["LeadName"]?.ToString() ?? "",
                    Subtitle = reader["MobileNo"]?.ToString() ?? "",
                    Url = "/Leads/Index",
                    Icon = "fa-solid fa-bullseye"
                });
            });

            return results.Take(limitPerType * 4).ToList();
        }

        private static void TrySearch(SqlConnection con, string sql, string pattern, Action<SqlDataReader> map)
        {
            try
            {
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Q", pattern);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    map(reader);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavbarSearch error: {ex.Message}");
            }
        }
    }

    public class NavbarSearchResult
    {
        public string Type { get; set; } = "";
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Url { get; set; } = "";
        public string Icon { get; set; } = "";
    }
}

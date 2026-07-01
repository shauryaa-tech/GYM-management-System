using GymManagement.Data.Queries;

using GymManagement.ViewModels;

using Microsoft.Data.SqlClient;



namespace GymManagement.Data.Repositories

{

    public class DashboardRepository

    {

        private readonly DbHelper _db;



        public DashboardRepository(DbHelper db)

        {

            _db = db;

        }



        public DashboardViewModel GetDashboardData(DateTime selectedDate)

        {

            selectedDate = selectedDate.Date;

            var model = new DashboardViewModel

            {

                SelectedDate = selectedDate,

                IsSelectedToday = selectedDate == DateTime.Today

            };



            using SqlConnection con = _db.GetConnection();

            con.Open();



            model.TotalMembers = ScalarInt(con, DashboardQueries.TotalMembers);

            model.ActiveMembers = ScalarInt(con, DashboardQueries.ActiveMembers);

            model.TodayAttendance = ScalarInt(con, DashboardQueries.TodayAttendance, selectedDate);

            model.TotalRevenue = ScalarDecimal(con, DashboardQueries.TotalRevenue);

            model.TodayRevenue = ScalarDecimal(con, DashboardQueries.TodayRevenue, selectedDate);

            model.MonthRevenue = ScalarDecimal(con, DashboardQueries.MonthRevenue, selectedDate);

            model.TotalStaff = ScalarInt(con, DashboardQueries.TotalStaff);

            model.ActiveStaff = ScalarInt(con, DashboardQueries.ActiveStaff);

            model.StaffAttendanceToday = ScalarInt(con, DashboardQueries.StaffAttendanceToday, selectedDate);

            model.PendingLeads = ScalarInt(con, DashboardQueries.PendingLeads);

            model.NewLeadsToday = ScalarInt(con, DashboardQueries.NewLeadsToday, selectedDate);

            model.FollowUpDue = ScalarInt(con, DashboardQueries.FollowUpDue, selectedDate);

            model.NewMembersThisMonth = ScalarInt(con, DashboardQueries.NewMembersThisMonth, selectedDate);



            LoadOutstandingStats(con, model);

            LoadExpiryStats(con, model, selectedDate);

            LoadRecentMembers(con, model);

            LoadExpiringMembers(con, model, selectedDate);

            LoadOutstandingRows(con, model);

            LoadRecentLeads(con, model);

            LoadFollowUpLeads(con, model, selectedDate);

            LoadMonthlyRevenue(con, model, selectedDate);

            LoadMemberAttendance(con, model, selectedDate);

            LoadTrainerAttendance(con, model, selectedDate);

            LoadPaymentModes(con, model, selectedDate);



            return model;

        }



        public (int TotalMembers, int ActiveMembers, decimal TodayRevenue) GetNavbarStats()

        {

            using SqlConnection con = _db.GetConnection();

            con.Open();

            var today = DateTime.Today;

            return (

                ScalarInt(con, DashboardQueries.TotalMembers),

                ScalarInt(con, DashboardQueries.ActiveMembers),

                ScalarDecimal(con, DashboardQueries.TodayRevenue, today)

            );

        }



        private static SqlCommand CreateCmd(SqlConnection con, string sql, DateTime? selectedDate = null)

        {

            var cmd = new SqlCommand(sql, con);

            if (selectedDate.HasValue)

                cmd.Parameters.AddWithValue("@SelectedDate", selectedDate.Value.Date);

            return cmd;

        }



        private static int ScalarInt(SqlConnection con, string sql, DateTime? selectedDate = null)

        {

            using var cmd = CreateCmd(con, sql, selectedDate);

            return Convert.ToInt32(cmd.ExecuteScalar());

        }



        private static decimal ScalarDecimal(SqlConnection con, string sql, DateTime? selectedDate = null)

        {

            using var cmd = CreateCmd(con, sql, selectedDate);

            return Convert.ToDecimal(cmd.ExecuteScalar());

        }



        private static void LoadOutstandingStats(SqlConnection con, DashboardViewModel model)

        {

            using var cmd = new SqlCommand(DashboardQueries.OutstandingStats, con);

            using var dr = cmd.ExecuteReader();

            if (!dr.Read()) return;

            model.TotalOutstanding = dr["TotalOutstanding"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["TotalOutstanding"]);

            model.OutstandingCount = dr["OutstandingCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["OutstandingCount"]);

        }



        private static void LoadExpiryStats(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.ExpiryStats, selectedDate);

            using var dr = cmd.ExecuteReader();

            if (!dr.Read()) return;

            model.ExpiredCount = dr["ExpiredCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ExpiredCount"]);

            model.ExpiringSoonCount = dr["ExpiringSoonCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ExpiringSoonCount"]);

        }



        private static void LoadRecentMembers(SqlConnection con, DashboardViewModel model)

        {

            using var cmd = new SqlCommand(DashboardQueries.RecentMembersDetailed, con);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.RecentMembers.Add(new DashboardMemberRow

                {

                    MemberName = dr["MemberName"]?.ToString() ?? "",

                    MemberCode = dr["MemberCode"]?.ToString(),

                    Status = dr["Status"]?.ToString() ?? "",

                    JoinDate = dr["JoinDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["JoinDate"])

                });

            }

        }



        private static void LoadExpiringMembers(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.ExpiringMembersTop, selectedDate);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.ExpiringMembers.Add(new DashboardExpiryRow

                {

                    MemberName = dr["MemberName"]?.ToString() ?? "",

                    PlanName = dr["PlanName"]?.ToString(),

                    PlanEndDate = dr["PlanEndDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["PlanEndDate"]),

                    DaysLeft = dr["DaysLeft"] == DBNull.Value ? 0 : Convert.ToInt32(dr["DaysLeft"])

                });

            }

        }



        private static void LoadOutstandingRows(SqlConnection con, DashboardViewModel model)

        {

            using var cmd = new SqlCommand(DashboardQueries.OutstandingTop, con);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.OutstandingRows.Add(new DashboardOutstandingRow

                {

                    MemberName = dr["MemberName"]?.ToString() ?? "",

                    PlanName = dr["PlanName"]?.ToString(),

                    Amount = Convert.ToDecimal(dr["Amount"]),

                    PaymentStatus = dr["PaymentStatus"]?.ToString() ?? ""

                });

            }

        }



        private static void LoadRecentLeads(SqlConnection con, DashboardViewModel model)

        {

            using var cmd = new SqlCommand(DashboardQueries.RecentLeadsTop, con);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.RecentLeads.Add(new DashboardLeadRow

                {

                    LeadId = Convert.ToInt32(dr["LeadId"]),

                    LeadName = dr["LeadName"]?.ToString() ?? "",

                    MobileNo = dr["MobileNo"]?.ToString(),

                    Status = dr["Status"]?.ToString(),

                    CreatedDate = dr["CreatedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedDate"])

                });

            }

        }



        private static void LoadFollowUpLeads(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.FollowUpLeadsTop, selectedDate);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.FollowUpLeads.Add(new DashboardLeadRow

                {

                    LeadId = Convert.ToInt32(dr["LeadId"]),

                    LeadName = dr["LeadName"]?.ToString() ?? "",

                    MobileNo = dr["MobileNo"]?.ToString(),

                    Status = dr["Status"]?.ToString(),

                    FollowUpDate = dr["FollowUpDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["FollowUpDate"])

                });

            }

        }



        private static void LoadMonthlyRevenue(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.MonthlyRevenueCombined, selectedDate);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.Months.Add(dr["MonthLabel"]?.ToString() ?? "");

                model.Revenues.Add(Convert.ToDecimal(dr["Revenue"]));

            }

        }



        private static void LoadMemberAttendance(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.MemberAttendance7Days, selectedDate);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.AttendanceDates.Add(dr["DateLabel"]?.ToString() ?? "");

                model.MemberAttendance.Add(Convert.ToInt32(dr["MemberCount"]));

            }

        }



        private static void LoadTrainerAttendance(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.TrainerAttendance7Days, selectedDate);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

                model.TrainerAttendance.Add(Convert.ToInt32(dr["TrainerCount"]));

        }



        private static void LoadPaymentModes(SqlConnection con, DashboardViewModel model, DateTime selectedDate)

        {

            using var cmd = CreateCmd(con, DashboardQueries.PaymentModeBreakdown, selectedDate);

            using var dr = cmd.ExecuteReader();

            while (dr.Read())

            {

                model.PaymentModes.Add(dr["PaymentMode"]?.ToString() ?? "Other");

                model.PaymentModeAmounts.Add(Convert.ToDecimal(dr["TotalAmount"]));

            }

        }

    }

}



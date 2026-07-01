using GymManagement.Data.Queries;
using GymManagement.ViewModels;
using Microsoft.Data.SqlClient;

namespace GymManagement.Data.Repositories
{
    public class NotificationRepository
    {
        private readonly DbHelper _db;

        public NotificationRepository(DbHelper db)
        {
            _db = db;
        }

        public List<NotificationItem> GetRecentNotifications(int limit = 10)
        {
            var notifications = new List<NotificationItem>();

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                // 1. Recent Leads (last 24 hours)
                try
                {
                    SqlCommand leadCmd = new SqlCommand(NotificationQueries.RecentLeads, con);
                    leadCmd.Parameters.AddWithValue("@Hours", 24);
                    SqlDataReader leadReader = leadCmd.ExecuteReader();
                    while (leadReader.Read())
                    {
                        notifications.Add(new NotificationItem
                        {
                            Id = $"lead-{leadReader["LeadId"]}",
                            Type = "lead",
                            Title = "New Lead",
                            Message = $"{leadReader["LeadName"]} - {leadReader["MobileNo"]}",
                            Time = Convert.ToDateTime(leadReader["CreatedDate"]),
                            Icon = "fas fa-user-plus",
                            Color = "text-primary",
                            BgColor = "bg-primary",
                            Url = "/Leads/Index"
                        });
                    }
                    leadReader.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lead notification error: {ex.Message}");
                }

                // 2. Recent Payments (last 24 hours)
                try
                {
                    SqlCommand paymentCmd = new SqlCommand(NotificationQueries.RecentPayments, con);
                    paymentCmd.Parameters.AddWithValue("@Hours", 24);
                    SqlDataReader paymentReader = paymentCmd.ExecuteReader();
                    while (paymentReader.Read())
                    {
                        notifications.Add(new NotificationItem
                        {
                            Id = $"payment-{paymentReader["PaymentId"]}",
                            Type = "payment",
                            Title = "Payment Received",
                            Message = $"{paymentReader["MemberName"]} - ₹{Convert.ToDecimal(paymentReader["Amount"]):N0}",
                            Time = Convert.ToDateTime(paymentReader["PaymentDate"]),
                            Icon = "fas fa-rupee-sign",
                            Color = "text-success",
                            BgColor = "bg-success",
                            Url = "/Payments/Index"
                        });
                    }
                    paymentReader.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment notification error: {ex.Message}");
                }

                // 3. New Members (last 24 hours)
                try
                {
                    SqlCommand memberCmd = new SqlCommand(NotificationQueries.RecentMembers, con);
                    memberCmd.Parameters.AddWithValue("@Hours", 24);
                    SqlDataReader memberReader = memberCmd.ExecuteReader();
                    while (memberReader.Read())
                    {
                        var memberId = Convert.ToInt32(memberReader["MemberId"]);
                        notifications.Add(new NotificationItem
                        {
                            Id = $"member-{memberId}",
                            Type = "member",
                            Title = "New Member",
                            Message = $"{memberReader["MemberName"]} - {memberReader["MemberCode"]}",
                            Time = Convert.ToDateTime(memberReader["JoinDate"]),
                            Icon = "fas fa-user-check",
                            Color = "text-info",
                            BgColor = "bg-info",
                            Url = $"/Members/Details/{memberId}"
                        });
                    }
                    memberReader.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Member notification error: {ex.Message}");
                }

                // 4. Expiring Memberships (next 7 days)
                try
                {
                    SqlCommand expiryCmd = new SqlCommand(NotificationQueries.ExpiringMemberships, con);
                    SqlDataReader expiryReader = expiryCmd.ExecuteReader();
                    while (expiryReader.Read())
                    {
                        var daysLeft = Convert.ToInt32(expiryReader["DaysLeft"]);
                        var memberId = Convert.ToInt32(expiryReader["MemberId"]);
                        notifications.Add(new NotificationItem
                        {
                            Id = $"expiry-{memberId}",
                            Type = "expiry",
                            Title = daysLeft <= 0 ? "Membership Expired" : "Membership Expiring Soon",
                            Message = $"{expiryReader["MemberName"]} - {expiryReader["PlanName"]} ({daysLeft} days)",
                            Time = Convert.ToDateTime(expiryReader["PlanEndDate"]),
                            Icon = daysLeft <= 0 ? "fas fa-exclamation-triangle" : "fas fa-clock",
                            Color = daysLeft <= 0 ? "text-danger" : "text-warning",
                            BgColor = daysLeft <= 0 ? "bg-danger" : "bg-warning",
                            Url = "/Reports/MembershipExpiry"
                        });
                    }
                    expiryReader.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Expiry notification error: {ex.Message}");
                }

                // Sort by time descending and take limit
                return notifications
                    .OrderByDescending(n => n.Time)
                    .Take(limit)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotificationRepository.GetRecentNotifications error: {ex.Message}");
                return new List<NotificationItem>();
            }
        }

        public int GetUnreadCount()
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                int count = 0;

                // Count recent items (last 24 hours)
                try
                {
                    SqlCommand leadCmd = new SqlCommand(NotificationQueries.CountRecentLeads, con);
                    leadCmd.Parameters.AddWithValue("@Hours", 24);
                    count += Convert.ToInt32(leadCmd.ExecuteScalar());
                }
                catch { }

                try
                {
                    SqlCommand paymentCmd = new SqlCommand(NotificationQueries.CountRecentPayments, con);
                    paymentCmd.Parameters.AddWithValue("@Hours", 24);
                    count += Convert.ToInt32(paymentCmd.ExecuteScalar());
                }
                catch { }

                try
                {
                    SqlCommand memberCmd = new SqlCommand(NotificationQueries.CountRecentMembers, con);
                    memberCmd.Parameters.AddWithValue("@Hours", 24);
                    count += Convert.ToInt32(memberCmd.ExecuteScalar());
                }
                catch { }

                try
                {
                    SqlCommand expiryCmd = new SqlCommand(NotificationQueries.CountExpiringMemberships, con);
                    count += Convert.ToInt32(expiryCmd.ExecuteScalar());
                }
                catch { }

                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotificationRepository.GetUnreadCount error: {ex.Message}");
                return 0;
            }
        }

        public List<NavbarMessageItem> GetFollowUpMessages(int limit = 10)
        {
            var messages = new List<NavbarMessageItem>();

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                SqlCommand cmd = new SqlCommand(NotificationQueries.FollowUpLeads, con);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var followUpDate = Convert.ToDateTime(reader["FollowUpDate"]);
                    var isOverdue = followUpDate.Date < DateTime.Today;

                    messages.Add(new NavbarMessageItem
                    {
                        LeadId = Convert.ToInt32(reader["LeadId"]),
                        Title = reader["LeadName"]?.ToString() ?? "",
                        Message = reader["Remarks"]?.ToString() ?? reader["MobileNo"]?.ToString() ?? "",
                        Time = followUpDate,
                        Status = reader["Status"]?.ToString() ?? "",
                        IsOverdue = isOverdue,
                        Url = "/Leads/Index"
                    });
                }
                reader.Close();

                return messages.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotificationRepository.GetFollowUpMessages error: {ex.Message}");
                return messages;
            }
        }

        public int GetFollowUpCount()
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();
                SqlCommand cmd = new SqlCommand(NotificationQueries.CountFollowUpLeads, con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                return 0;
            }
        }
    }
}
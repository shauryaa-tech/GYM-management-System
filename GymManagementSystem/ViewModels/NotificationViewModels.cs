namespace GymManagement.ViewModels
{
    public class NotificationItem
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Time { get; set; }
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";
        public string BgColor { get; set; } = "";
        public string Url { get; set; } = "/Dashboard/Index";
    }

    public class NavbarMessageItem
    {
        public int LeadId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Time { get; set; }
        public string Status { get; set; } = "";
        public bool IsOverdue { get; set; }
        public string Url { get; set; } = "";
    }
}

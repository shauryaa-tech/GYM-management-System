namespace GymManagement.Helpers
{
    public static class TimeFormatHelper
    {
        public static string? FormatTime12(TimeSpan? time)
        {
            if (time == null) return null;
            try
            {
                return DateTime.Today.Add(time.Value).ToString("hh:mm tt");
            }
            catch (FormatException)
            {
                return time.Value.ToString(@"hh\:mm");
            }
        }

        public static string? FormatTime24(TimeSpan? time)
        {
            if (time == null) return null;
            return time.Value.ToString(@"hh\:mm");
        }

        public static TimeSpan? ReadTime(object? value)
        {
            if (value == null || value is DBNull) return null;
            if (value is TimeSpan ts) return ts;
            if (value is DateTime dt) return dt.TimeOfDay;
            if (value is TimeOnly to) return to.ToTimeSpan();
            return TimeSpan.TryParse(value.ToString(), out var parsed) ? parsed : null;
        }
    }
}

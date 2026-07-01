namespace GymManagement.Models
{
    public class StaffAttendance
    {
        public int AttendanceId { get; set; }
        public int StaffId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        public string? StaffName { get; set; }
        public string? Designation { get; set; }

        public string? FormattedCheckIn => GymManagement.Helpers.TimeFormatHelper.FormatTime12(CheckInTime);
        public string? FormattedCheckOut => GymManagement.Helpers.TimeFormatHelper.FormatTime12(CheckOutTime);
    }
}

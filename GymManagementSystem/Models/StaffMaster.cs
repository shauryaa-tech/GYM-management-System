namespace GymManagement.Models
{
    public class StaffMaster
    {
        public int StaffId { get; set; }

        public string StaffName { get; set; } = "";

        public string Gender { get; set; } = "";

        public string MobileNo { get; set; } = "";

        public string Email { get; set; } = "";

        public string Designation { get; set; } = "";

        public string? Specializations { get; set; }

        public int? ExperienceYears { get; set; }

        public decimal Salary { get; set; }

        public DateTime JoiningDate { get; set; }

        public string Address { get; set; } = "";

        public bool IsActive { get; set; }

        public int RoleId { get; set; }

        // Display Purpose
        public string? RoleName { get; set; }

        /// <summary>Unique code for biometric / payroll (e.g. EMP043).</summary>
        public string? StaffCode { get; set; }

        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? IfscCode { get; set; }

        public string DisplayStaffCode =>
            string.IsNullOrWhiteSpace(StaffCode) ? $"EMP{StaffId:D3}" : StaffCode!;
    }
}
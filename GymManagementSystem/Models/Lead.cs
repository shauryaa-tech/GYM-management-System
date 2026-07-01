namespace GymManagement.Models
{
    public class Lead
    {
        public int LeadId { get; set; }

        public string LeadCode { get; set; } = "";

        public string LeadName { get; set; } = "";

        public string MobileNo { get; set; } = "";

        public string? AlternateMobile { get; set; }

        public string? Email { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        public string? InterestedIn { get; set; }

        public int LeadSourceId { get; set; }

        public string? LeadSourceName { get; set; }

        public int? AssignedTo { get; set; }

        public string? AssignedStaffName { get; set; }

        public string Status { get; set; } = "New";

        public decimal? Budget { get; set; }

        public string? Remarks { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public bool IsConverted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
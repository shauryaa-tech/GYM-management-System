namespace GymManagement.Models
{
    public class LeadSourceMaster
    {
        public int LeadSourceId { get; set; }

        public string SourceCode { get; set; } = "";

        public string SourceName { get; set; } = "";

        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Dashboard Statistics
        public int LeadCount { get; set; }
    }
}
namespace GymManagement.Models
{
    public class MemberMaster
    {
        public int MemberId { get; set; }

        public string MemberCode { get; set; } = "";

        public string MemberName { get; set; } = "";

        public string MobileNo { get; set; } = "";

        public string? AlternateMobile { get; set; }

        public string? Email { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? BloodGroup { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }

        public string? EmergencyContact { get; set; }

        public string? EmergencyContactName { get; set; }

        public int? TrainerId { get; set; }

        public int? PlanId { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime PlanStartDate { get; set; }

        public DateTime PlanEndDate { get; set; }

        public decimal Height { get; set; }

        public decimal Weight { get; set; }

        public string Status { get; set; } = "Active";

        public string? MedicalNotes { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public string? TrainerName { get; set; }

        public string? PlanName { get; set; }
    }
}
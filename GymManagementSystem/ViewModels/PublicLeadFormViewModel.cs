using System.ComponentModel.DataAnnotations;

namespace GymManagement.ViewModels
{
    public class PublicLeadFormViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10-digit mobile number")]
        [Display(Name = "Mobile Number")]
        public string MobileNo { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter valid email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Interested In")]
        public string? InterestedIn { get; set; }
    }
}

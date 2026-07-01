namespace GymManagement.Models
{
    public class VendorMaster
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = "";
        public string? ContactPerson { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? GSTNo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

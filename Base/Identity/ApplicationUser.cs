using Microsoft.AspNetCore.Identity;

namespace CreditFlowAPI.Base.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Enhanced KYC Profile Information
        public string? TaxId { get; set; }  // SSN (masked)
        public DateTime? DateOfBirth { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }
}

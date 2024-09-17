using Microsoft.AspNetCore.Identity;

namespace userIdentityAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        //public string? DisplayName { get; set; }
        //public int Karma {  get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}

namespace userIdentityAPI.DTOs
{
    public class UpdateUserProfileDto
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}

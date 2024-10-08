namespace post.DTOs
{
    public class CreatePostDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}

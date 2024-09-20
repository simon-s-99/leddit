namespace post.Models.DTOs
{
    public class CreatePostDTO
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public Guid UserId { get; set; }
    }
}

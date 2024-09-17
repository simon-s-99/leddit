namespace post.Models.DTOs
{
    public class CreatePostDTO
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public int UserId { get; set; }
    }
}

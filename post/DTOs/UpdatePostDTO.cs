using System.ComponentModel.DataAnnotations;

namespace post.DTOs
{
    public class UpdatePostDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}

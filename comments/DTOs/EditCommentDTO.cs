using System.ComponentModel.DataAnnotations;

namespace Comments.DTOs
{
    public class EditCommentDTO
    {
        [Required]
        [MaxLength(500)]
        [Editable(true)]
        public string Body { get; set; } = string.Empty;
    }
}

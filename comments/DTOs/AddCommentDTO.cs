using System.ComponentModel.DataAnnotations;

namespace Comments.DTOs
{
    public class AddCommentDTO
    {
        //[Required]
        //public Guid AuthorId { get; set; }

        //[Required]
        //public Guid PostId { get; set; }

        public Guid? ReplyTo { get; set; }

        [Required]
        [MaxLength(500)]
        [Editable(true)]
        public string Body { get; set; } = string.Empty;
    }
}

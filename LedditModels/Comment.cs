using System.ComponentModel.DataAnnotations;

namespace LedditModels
{
    public class Comment
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        //[Required]
        //public Guid AuthorId { get; set; }

        //[Required]
        //public Guid PostId { get; set; }

        public Guid? ReplyTo { get; set; } // Optional, the id of the comment that the original comment is in reply to

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        [MaxLength(500)]
        [Editable(true)]
        public string Body { get; set; } = string.Empty;
    }
}

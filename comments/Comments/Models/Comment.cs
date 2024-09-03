using System.ComponentModel.DataAnnotations;

namespace Comments.Models
{
    public class Comment
    {
        [Required]
        public Guid Id { get; set; }
        //[Required]
        //public User Author { get; set; }
        //[Required]
        //public Post Post { get; set; }
        public Comment? ReplyTo { get; set; } // Optional, the comment that the origional comment is in reply to
        [Required]
        [MaxLength(500)]
        [Editable(true)]
        public string Body { get; set; }
    }
}

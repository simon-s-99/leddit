using Comments.Models;
using System.ComponentModel.DataAnnotations;

namespace Comments.Data.DTOs
{
    public class AddCommentDTO
    {
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

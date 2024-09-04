using Comments.Models;
using System.ComponentModel.DataAnnotations;

namespace Comments.Models.DTOs
{
    public class AddCommentDTO
    {
        //[Required]
        //public User Author { get; set; }
        //[Required]
        //public Post Post { get; set; }
        public Guid? ReplyTo { get; set; }
        [Required]
        [MaxLength(500)]
        [Editable(true)]
        public string Body { get; set; }
    }
}

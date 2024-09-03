using Comments.Models;
using System.ComponentModel.DataAnnotations;

namespace Comments.Data.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        //public User Author { get; set; }
        //public Post Post { get; set; }
        public Comment? ReplyTo { get; set; } // Optional, the comment that the origional comment is in reply to
        public string Body { get; set; }

        public CommentDto(Comment comment)
        {
            Id = comment.Id;
            //Author = comment.Author;
            //Post = comment.Post;
            ReplyTo = comment.ReplyTo ?? null;
            Body = comment.Body;
        }
    }
}

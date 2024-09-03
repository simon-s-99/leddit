using Comments.Data;
using Comments.Models;
using Comments.Models.DTOs;

namespace Comments.Services
{
    public class CommentsService
    {
        private ApplicationDbContext _context;

        public CommentsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Comment AddComment(AddCommentDTO comment)
        {
            //TODO: Add validation middleware

            Comment newComment = new Comment
            {
                //Author = comment.Author,
                //Post = comment.Post,
                ReplyTo = comment.ReplyTo ?? null,
                Body = comment.Body,

            };

            _context.Comments.Add(newComment);
            _context.SaveChanges();

            return newComment;
        }

        public 
    }
}

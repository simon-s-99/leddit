using Comments.Data;
using Comments.Models;

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
                Body = comment.Body,
            };
        }
    }
}

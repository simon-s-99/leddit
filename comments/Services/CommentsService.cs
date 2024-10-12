using Comments.Repository;
using LedditModels;
using Microsoft.IdentityModel.Tokens;
using System.Web.Http;
using Comments.DTOs;

namespace Comments.Services
{
    public class CommentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly MessageService _messageService;

        public CommentsService(ApplicationDbContext context, MessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        public Comment AddComment(AddCommentDTO comment)
        {
            if (comment.Body.IsNullOrEmpty())
            {
                // Throw a new 404 response if comment body is empty
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            Comment newComment = new Comment
            {
                UserId = comment.UserId,
                PostId = comment.PostId,
                ReplyTo = comment.ReplyTo ?? null, // If reply exists, add it, otherwise send null
                DateCreated = DateTime.UtcNow,
                Body = comment.Body,
            };

            // Check if post and user exist
            bool postExists = _messageService.CheckObjectExists(newComment.PostId, "post-api-svc");
            bool userExists = _messageService.CheckObjectExists(newComment.UserId, "user-api-svc");

            if (!postExists || !userExists)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            // If comment is replying to a supposed comment, check that it actually exists
            if (comment.ReplyTo is not null)
            {
                bool replyCommentExists = _context.Comments.Where(c => c.Id == comment.ReplyTo).Any();

                if (!replyCommentExists)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
                }
            }

            _context.Comments.Add(newComment);
            _context.SaveChanges();

            // Send message to the "add-comment" exchange
            _messageService.NotifyCommentChanged("add-comment", newComment);

            return newComment;
        }

        public Comment DeleteComment(Guid id)
        {
            var commentToDelete = _context.Comments.Where(c => c.Id == id).FirstOrDefault();

            if (commentToDelete is null)
            {
                // Throw a new 404 response if no comment was found
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            _context.Comments.Remove(commentToDelete);
            _context.SaveChanges();

            _messageService.NotifyCommentChanged("delete-comment", commentToDelete);

            return commentToDelete;
        }

        public Comment EditComment(Guid id, EditCommentDTO comment)
        {
            var commentToUpdate = _context.Comments.Where(c => c.Id == id).FirstOrDefault();

            if (comment.Body.IsNullOrEmpty() || commentToUpdate is null)
            {
                // Throw a new 404 response if comment body is empty
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            commentToUpdate.Body = comment.Body;
            commentToUpdate.DateCreated = DateTime.UtcNow;
            _context.SaveChanges();

            _messageService.NotifyCommentChanged("edit-comment", commentToUpdate);

            return commentToUpdate;
        }

        public List<Comment> GetCommentsFromPostId(Guid id)
        {
            List<Comment> postComments = _context.Comments.Where(c => c.PostId == id).ToList();
            return postComments;
        }
    }
}

using Comments.Data;
using Comments.Models;
using Comments.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Web.Http;

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
                //Author = comment.Author,
                //Post = comment.Post,
                ReplyTo = comment.ReplyTo ?? null, // If reply exists, add it, otherwise send null
                Body = comment.Body,
            };

            if (newComment.ReplyTo is not null)
            {
                // If reply id is not null but the reply itself is null, throw an HTTP exception
                var reply = _context.Comments.Where(c => c.Id == newComment.Id).FirstOrDefault() ?? throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
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
            _context.SaveChanges();

            _messageService.NotifyCommentChanged("edit-comment", commentToUpdate);

            return commentToUpdate;
        }
    }
}

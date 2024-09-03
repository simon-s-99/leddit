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

        public CommentsService(ApplicationDbContext context)
        {
            _context = context;
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
                ReplyTo = comment.ReplyTo ?? null,
                Body = comment.Body,

            };

            _context.Comments.Add(newComment);
            _context.SaveChanges();

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

            return commentToDelete;
        }

        public Comment EditComment(Guid id, EditCommentDTO comment)
        {
            Comment commentToUpdate = _context.Comments.Where(c => c.Id == id).FirstOrDefault();

            if (comment.Body.IsNullOrEmpty() || commentToUpdate is null)
            {
                // Throw a new 404 response if comment body is empty
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            commentToUpdate.Body = comment.Body;
            _context.SaveChanges();

            return commentToUpdate;
        }
    }
}

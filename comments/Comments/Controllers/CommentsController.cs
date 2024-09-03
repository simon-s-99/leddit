using Comments.Data;
using Comments.Data.DTOs;
using Comments.Models;
using Comments.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comments.Controllers
{
    [ApiController]
    [Route("/comments")]
    public class CommentsController : ControllerBase
    {
        private CommentsService _service;

        public CommentsController(CommentsService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult<AddCommentDTO> AddComment([FromBody] AddCommentDTO comment)
        {
            try
            {
                Comment newComment = _service.AddComment(comment);
                return Ok(new CommentDto(newComment));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

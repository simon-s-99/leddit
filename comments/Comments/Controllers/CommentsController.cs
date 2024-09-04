using Comments.Data;
using Comments.Models;
using Comments.Models.DTOs;
using Comments.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comments.Controllers
{
    [ApiController]
    [Route("/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentsService _service;

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
                return Ok(newComment);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        //TODO: Handle user authorization
        public ActionResult<Comment> DeleteComment([FromQuery] Guid id)
        {
            try
            {
                Comment commentToDelete = _service.DeleteComment(id);
                return Ok(commentToDelete);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch]
        //TODO: Handle user authorization
        public ActionResult<Comment> EditComment([FromQuery] Guid id, [FromBody] EditCommentDTO comment)
        {
            try
            {
                Comment commentToUpdate = _service.EditComment(id, comment);
                return Ok(commentToUpdate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

using LedditModels;
using Comments.Services;
using Microsoft.AspNetCore.Mvc;
using Comments.DTOs;

namespace Comments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        //[Authorize(Roles = "Author,Administrator")]
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
        //[Authorize(Roles = "Author")]
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

        [HttpGet("crash")]
        public IActionResult CrashApp()
        {
            Environment.Exit(1);
            return Ok("App crashed successfully");
        }
    }
}

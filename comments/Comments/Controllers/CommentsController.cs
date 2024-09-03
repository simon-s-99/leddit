using Comments.Data;
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
        public IEnumerable<Comment> AddComment([FromBody] AddCommentDTO comment)
        {
            
        }
    }
}

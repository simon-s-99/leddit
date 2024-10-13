using Microsoft.AspNetCore.Mvc;
using post.Services;
using post.DTOs;

namespace post.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;

        public PostsController(PostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDTO dto)
        {
            try
            {
                var postId = await _postService.CreatePostAsync(dto);
                return StatusCode(201, new { Id = postId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePost([FromQuery] Guid id, [FromBody] UpdatePostDTO dto)
        {
            try
            {
                await _postService.UpdatePostAsync(id, dto);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePost([FromQuery] Guid id)
        {
            try
            {
                await _postService.DeletePostAsync(id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPost([FromQuery] Guid id)
        {
            try
            {
                var post = await _postService.GetPostAsync(id);
                return Ok(post);
            }
            catch (System.Exception ex)
            {
                return NotFound(new { Error = ex.Message });
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


﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using post.Models;
using post.Models.DTOs;
using post.Data;
using post.Services;

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
                return Ok(new { Id = postId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDTO dto)
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
    }
}

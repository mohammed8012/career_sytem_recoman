using career_sytem_recoman.Models.DTOs.Posts;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpGet("feed")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeed()
        {
            var feed = await _postService.GetFeedAsync();
            return Ok(feed);
        }

        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { Message = "Post not found." }); // <-- 404
            return Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var companyId = GetCurrentUserId();
            if (companyId == 0)
                return Unauthorized();

            var post = await _postService.CreatePostAsync(companyId, dto);
            return CreatedAtAction(nameof(GetPost), new { postId = post.PostId }, post);
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] CreatePostDto dto)
        {
            var companyId = GetCurrentUserId();
            if (companyId == 0)
                return Unauthorized();

            var updated = await _postService.UpdatePostAsync(postId, companyId, dto);
            return Ok(updated);
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var companyId = GetCurrentUserId();
            if (companyId == 0)
                return Unauthorized();

            await _postService.DeletePostAsync(postId, companyId);
            return NoContent();
        }
    }
}
using career_sytem_recoman.Models.DTOs.User;
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
        public async Task<IActionResult> GetFeed()
        {
            var feed = await _postService.GetFeedAsync(GetCurrentUserId());
            return Ok(feed);
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] CreatePostDto dto)
        {
            var updated = await _postService.UpdatePostAsync(postId, dto, GetCurrentUserId());
            return Ok(updated);
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            await _postService.DeletePostAsync(postId, GetCurrentUserId());
            return NoContent();
        }
    }
}
using career_sytem_recoman.Models.DTOs.Rating;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _ratingService.CreateRatingAsync(dto, userId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserRatings(int userId)
        {
            var ratings = await _ratingService.GetRatingsForUserAsync(userId);
            return Ok(ratings);
        }
    }
}
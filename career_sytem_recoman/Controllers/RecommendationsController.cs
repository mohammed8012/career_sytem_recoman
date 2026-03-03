using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetRecommendedJobs()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var jobs = await _recommendationService.GetRecommendedJobsAsync(userId);
            return Ok(jobs);
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetRecommendedCourses()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var courses = await _recommendationService.GetRecommendedCoursesAsync(userId);
            return Ok(courses);
        }
    }
}
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        private readonly IRecommendationService _recommendationService;

        public HomeController(IHomeService homeService, IRecommendationService recommendationService)
        {
            _homeService = homeService;
            _recommendationService = recommendationService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : (int?)null;
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                // مستخدم مسجل → نعرض توصيات مخصصة
                var recommendedJobs = await _recommendationService.GetRecommendedJobsAsync(userId.Value);
                var recommendedCourses = await _recommendationService.GetRecommendedCoursesAsync(userId.Value);

                return Ok(new
                {
                    Jobs = recommendedJobs,
                    Courses = recommendedCourses
                });
            }
            else
            {
                // زائر غير مسجل → نعرض آخر العناصر (الطريقة القديمة)
                var suggestions = await _homeService.GetSuggestionsAsync();
                return Ok(suggestions);
            }
        }
    }
}
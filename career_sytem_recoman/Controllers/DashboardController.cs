using career_sytem_recoman.Models.DTOs.Dashboard;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب تسجيل الدخول
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRecommendationService _recommendationService;

        public DashboardController(IUserService userService, IRecommendationService recommendationService)
        {
            _userService = userService;
            _recommendationService = recommendationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        /// <summary>
        /// جلب نظرة شاملة للمستخدم: تحليل السيرة، المهارات، الوظائف المناسبة، الكورسات المناسبة.
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            // 1. جلب الملف الشخصي (يحتوي على التحليل والمهارات)
            var profile = await _userService.GetProfileAsync(userId);

            // 2. جلب توصيات الوظائف والكورسات
            var recommendedJobs = await _recommendationService.GetRecommendedJobsAsync(userId);
            var recommendedCourses = await _recommendationService.GetRecommendedCoursesAsync(userId);

            var result = new DashboardOverviewDto
            {
                Analysis = profile.CvAnalysis,
                Skills = profile.SkillsList,
                RecommendedJobs = recommendedJobs,
                RecommendedCourses = recommendedCourses
            };

            return Ok(result);
        }
    }
}
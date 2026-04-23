using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SavedJobsController : ControllerBase
    {
        private readonly ISavedJobService _savedJobService;

        public SavedJobsController(ISavedJobService savedJobService)
        {
            _savedJobService = savedJobService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        /// <summary>
        /// حفظ وظيفة للمستخدم الحالي
        /// </summary>
        [HttpPost("{jobId}")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            await _savedJobService.SaveJobAsync(userId, jobId);
            return Ok(new { message = "Job saved successfully." });
        }

        /// <summary>
        /// إلغاء حفظ وظيفة
        /// </summary>
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> UnsaveJob(int jobId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            await _savedJobService.UnsaveJobAsync(userId, jobId);
            return Ok(new { message = "Job unsaved successfully." });
        }

        /// <summary>
        /// جلب قائمة الوظائف المحفوظة للمستخدم الحالي
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSavedJobs()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var savedJobs = await _savedJobService.GetSavedJobsAsync(userId);
            return Ok(savedJobs);
        }

        /// <summary>
        /// التحقق مما إذا كانت وظيفة معينة محفوظة
        /// </summary>
        [HttpGet("check/{jobId}")]
        public async Task<IActionResult> IsJobSaved(int jobId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var isSaved = await _savedJobService.IsJobSavedAsync(userId, jobId);
            return Ok(new { isSaved });
        }
    }
}
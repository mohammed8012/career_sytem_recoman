using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private bool IsCompany()
        {
            return User.IsInRole("Company") || User.IsInRole("Employer");
        }

        /// <summary>
        /// تقديم على وظيفة (للباحثين عن عمل فقط)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Apply([FromBody] CreateApplicationDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var result = await _applicationService.ApplyAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// جلب تقديمات المستخدم الحالي
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserApplications(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0 || currentUserId != userId)
                return Forbid();

            var applications = await _applicationService.GetUserApplicationsAsync(userId);
            return Ok(applications);
        }

        /// <summary>
        /// جلب المتقدمين لوظيفة معينة (لأصحاب العمل فقط)
        /// </summary>
        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetJobApplications(int jobId)
        {
            if (!IsCompany())
                return Forbid();

            var employerId = GetCurrentUserId();
            if (employerId == 0)
                return Unauthorized();

            var applications = await _applicationService.GetJobApplicationsAsync(jobId, employerId);
            return Ok(applications);
        }

        /// <summary>
        /// تحديث حالة التقديم (لأصحاب العمل فقط)
        /// </summary>
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateApplicationStatusDto dto)
        {
            if (!IsCompany())
                return Forbid();

            var employerId = GetCurrentUserId();
            if (employerId == 0)
                return Unauthorized();

            var result = await _applicationService.UpdateApplicationStatusAsync(dto.ApplicationId, dto, employerId);
            return Ok(result);
        }
    }
}
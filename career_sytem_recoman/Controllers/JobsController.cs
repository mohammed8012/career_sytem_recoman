using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto filter)
        {
            var jobs = await _jobService.GetJobsAsync(filter);
            return Ok(jobs);
        }

        [HttpGet("{jobId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJob(int jobId)
        {
            var job = await _jobService.GetJobAsync(jobId);
            return Ok(job);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            var employerId = GetCurrentUserId();
            var job = await _jobService.CreateJobAsync(dto, employerId);
            return Ok(job);
        }

        [HttpPut("{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, [FromBody] UpdateJobDto dto)
        {
            var employerId = GetCurrentUserId();
            var job = await _jobService.UpdateJobAsync(jobId, dto, employerId);
            return Ok(job);
        }

        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            var employerId = GetCurrentUserId();
            await _jobService.DeleteJobAsync(jobId, employerId);
            return NoContent();
        }

        [HttpGet("company/{companyId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobsByCompany(int companyId)
        {
            var jobs = await _jobService.GetJobsByCompanyAsync(companyId);
            return Ok(jobs);
        }
    }
}
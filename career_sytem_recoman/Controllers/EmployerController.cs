using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Employer")]
    public class EmployerController(IEmployerService employerService) : ControllerBase
    {
        private readonly IEmployerService _employerService = employerService;

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            var userId = GetCurrentUserId();
            var jobs = await _employerService.GetJobsByEmployerAsync(userId);
            return Ok(jobs);
        }

        [HttpGet("jobs/{jobId}/applicants")]
        public async Task<IActionResult> GetJobApplicants(int jobId)
        {
            var userId = GetCurrentUserId();
            var applicants = await _employerService.GetApplicantsForJobAsync(jobId, userId);
            return Ok(applicants);
        }

        [HttpGet("applicants/{applicantId}/cv")]
        public async Task<IActionResult> DownloadApplicantCv(int applicantId)
        {
            var userId = GetCurrentUserId();
            var file = await _employerService.GetApplicantCvAsync(applicantId, userId);
            if (file.Stream == null) return NotFound();
            return File(file.Stream, file.ContentType, file.FileName);
        }
    }
}
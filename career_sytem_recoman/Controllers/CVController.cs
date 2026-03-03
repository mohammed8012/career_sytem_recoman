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
    public class CVController : ControllerBase
    {
        private readonly IAiCvService _aiCvService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public CVController(IAiCvService aiCvService, IUserService userService, IWebHostEnvironment env)
        {
            _aiCvService = aiCvService;
            _userService = userService;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSavedCv()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var user = await _userService.GetProfileAsync(userId);
            if (string.IsNullOrEmpty(user.Cvpath))
                return BadRequest("No CV found for this user. Please upload a CV first.");

            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, user.Cvpath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound("Saved CV file not found on server.");

            await using var fileStream = System.IO.File.OpenRead(filePath);
            var result = await _aiCvService.GetFullAnalysisAsync(fileStream, Path.GetFileName(filePath));

            var updateDto = new UpdateProfileDto
            {
                CvAnalysis = result.Analysis,
                SkillsList = result.Skills
            };
            await _userService.UpdateProfileAsync(userId, updateDto);

            return Ok(new
            {
                Message = "Saved CV analyzed and saved successfully.",
                Analysis = result.Analysis,
                Skills = result.Skills
            });
        }

        [HttpPost("upload-and-analyze")]
        public async Task<IActionResult> UploadAndAnalyzeCv(IFormFile file, [FromForm] bool save = false)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".pdf")
                return BadRequest("Only PDF files are allowed.");

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            await using var fileStream = file.OpenReadStream();
            var result = await _aiCvService.GetFullAnalysisAsync(fileStream, file.FileName);

            if (save)
            {
                var updateDto = new UpdateProfileDto
                {
                    CvAnalysis = result.Analysis,
                    SkillsList = result.Skills
                };
                await _userService.UpdateProfileAsync(userId, updateDto);
            }

            return Ok(new
            {
                Message = save ? "File analyzed and saved." : "File analyzed (not saved).",
                Analysis = result.Analysis,
                Skills = result.Skills
            });
        }

        [HttpGet("analysis")]
        public async Task<IActionResult> GetMyAnalysis()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var profile = await _userService.GetProfileAsync(userId);
            return Ok(new
            {
                Analysis = profile.CvAnalysis ?? "",
                Skills = profile.SkillsList ?? new List<string>()
            });
        }

        [HttpGet("analysis/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAnalysis(int userId)
        {
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(new
            {
                Analysis = profile.CvAnalysis ?? "",
                Skills = profile.SkillsList ?? new List<string>()
            });
        }
    }
}
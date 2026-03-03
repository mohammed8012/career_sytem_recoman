using career_sytem_recoman.Models.DTOs.User;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

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

        /// <summary>
        /// رفع سيرة ذاتية جديدة وتحليلها (بدون حفظ التحليل)
        /// </summary>
        [HttpPost("extract")]
        public async Task<IActionResult> ExtractAnalysisFromCv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".pdf")
                return BadRequest("Only PDF files are allowed.");

            using var stream = file.OpenReadStream();
            var result = await _aiCvService.GetFullAnalysisAsync(stream, file.FileName);

            return Ok(new
            {
                Analysis = result.Analysis,
                Skills = result.Skills
            });
        }

        /// <summary>
        /// رفع سيرة ذاتية جديدة وتحليلها وحفظ التحليل والمهارات في قاعدة البيانات
        /// </summary>
        [HttpPost("extract-and-save")]
        public async Task<IActionResult> ExtractAndSaveAnalysis(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".pdf")
                return BadRequest("Only PDF files are allowed.");

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            using var stream = file.OpenReadStream();
            var result = await _aiCvService.GetFullAnalysisAsync(stream, file.FileName);

            var updateDto = new UpdateProfileDto
            {
                CvAnalysis = result.Analysis,
                SkillsList = result.Skills
            };
            await _userService.UpdateProfileAsync(userId, updateDto);

            return Ok(new
            {
                Message = "Analysis and skills saved successfully.",
                Analysis = result.Analysis,
                Skills = result.Skills
            });
        }

        /// <summary>
        /// تحليل السيرة الذاتية المحفوظة مسبقاً للمستخدم الحالي (من Cvpath)
        /// </summary>
        [HttpPost("analyze-saved-cv")]
        public async Task<IActionResult> AnalyzeSavedCv()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            // 1. جلب ملف السيرة الذاتية للمستخدم
            var user = await _userService.GetProfileAsync(userId);
            if (string.IsNullOrEmpty(user.Cvpath))
                return BadRequest("No CV found for this user. Please upload a CV first.");

            // 2. قراءة الملف من الخادم
            string webRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            var filePath = Path.Combine(webRootPath, user.Cvpath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound("CV file not found on server.");

            // 3. إرسال الملف إلى خدمة الذكاء الاصطناعي
            await using var fileStream = System.IO.File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);
            var result = await _aiCvService.GetFullAnalysisAsync(fileStream, fileName);

            // 4. حفظ التحليل والمهارات في قاعدة البيانات
            var updateDto = new UpdateProfileDto
            {
                CvAnalysis = result.Analysis,
                SkillsList = result.Skills
            };
            await _userService.UpdateProfileAsync(userId, updateDto);

            return Ok(new
            {
                Message = "Saved CV analyzed and updated successfully.",
                Analysis = result.Analysis,
                Skills = result.Skills
            });
        }

        /// <summary>
        /// جلب التحليل المحفوظ للمستخدم الحالي
        /// </summary>
        [HttpGet("analysis")]
        public async Task<IActionResult> GetMyAnalysis()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var profile = await _userService.GetProfileAsync(userId);
            return Ok(new
            {
                Analysis = profile.CvAnalysis ?? "",
                Skills = profile.SkillsList ?? new List<string>()
            });
        }

        /// <summary>
        /// جلب التحليل المحفوظ لمستخدم محدد
        /// </summary>
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
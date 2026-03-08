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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public UsersController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateProfileDto dto)
        {
            if (userId != GetCurrentUserId())
                return Forbid();

            var result = await _userService.UpdateProfileAsync(userId, dto);
            return Ok(result);
        }

        [HttpPost("{userId}/cv")]
        public async Task<IActionResult> UploadCv(int userId, [FromForm] UploadCvDto dto)
        {
            if (userId != GetCurrentUserId())
                return Forbid();

            var result = await _userService.UploadCvAsync(userId, dto.CvFile);
            return Ok(new { cvPath = result });
        }

        [HttpGet("{userId}/cv")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCv(int userId)
        {
            var file = await _userService.GetCvFileAsync(userId);
            if (file.Stream == null) return NotFound();
            return File(file.Stream, file.ContentType, file.FileName);
        }

        // =============== دوال الصور الجديدة ===============

        /// <summary>
        /// رفع صورة شخصية للمستخدم (jpg, png, gif)
        /// </summary>
        [HttpPost("{userId}/profile-image")]
        public async Task<IActionResult> UploadProfileImage(int userId, IFormFile file)
        {
            if (userId != GetCurrentUserId())
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                return BadRequest("Only image files (jpg, png, gif) are allowed.");

            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_profile_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // جلب البيانات الحالية للمستخدم (لحذف الصورة القديمة)
            var currentProfile = await _userService.GetProfileAsync(userId);
            if (!string.IsNullOrEmpty(currentProfile.ProfileImagePath))
            {
                var oldPath = Path.Combine(webRootPath, currentProfile.ProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var updateDto = new UpdateProfileDto
            {
                ProfileImagePath = $"/uploads/profiles/{fileName}"
            };
            var updated = await _userService.UpdateProfileAsync(userId, updateDto);

            return Ok(new { profileImagePath = updated.ProfileImagePath });
        }

        /// <summary>
        /// رفع صورة غلاف للمستخدم (jpg, png, gif)
        /// </summary>
        [HttpPost("{userId}/cover-image")]
        public async Task<IActionResult> UploadCoverImage(int userId, IFormFile file)
        {
            if (userId != GetCurrentUserId())
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                return BadRequest("Only image files (jpg, png, gif) are allowed.");

            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "covers");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_cover_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // جلب البيانات الحالية للمستخدم (لحذف الصورة القديمة)
            var currentProfile = await _userService.GetProfileAsync(userId);
            if (!string.IsNullOrEmpty(currentProfile.CoverImagePath))
            {
                var oldPath = Path.Combine(webRootPath, currentProfile.CoverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var updateDto = new UpdateProfileDto
            {
                CoverImagePath = $"/uploads/covers/{fileName}"
            };
            var updated = await _userService.UpdateProfileAsync(userId, updateDto);

            return Ok(new { coverImagePath = updated.CoverImagePath });
        }

        /// <summary>
        /// تحميل الصورة الشخصية لمستخدم معين
        /// </summary>
        [HttpGet("{userId}/profile-image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileImage(int userId)
        {
            var profile = await _userService.GetProfileAsync(userId);
            if (string.IsNullOrEmpty(profile.ProfileImagePath))
                return NotFound();

            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, profile.ProfileImagePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var stream = System.IO.File.OpenRead(filePath);
            var ext = Path.GetExtension(filePath).ToLower();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            return File(stream, contentType);
        }

        /// <summary>
        /// تحميل صورة الغلاف لمستخدم معين
        /// </summary>
        [HttpGet("{userId}/cover-image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoverImage(int userId)
        {
            var profile = await _userService.GetProfileAsync(userId);
            if (string.IsNullOrEmpty(profile.CoverImagePath))
                return NotFound();

            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, profile.CoverImagePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var stream = System.IO.File.OpenRead(filePath);
            var ext = Path.GetExtension(filePath).ToLower();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            return File(stream, contentType);
        }
    }
}
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
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

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
            return Ok(result);
        }

        [HttpGet("{userId}/cv")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCv(int userId)
        {
            var file = await _userService.GetCvFileAsync(userId);
            if (file.Stream == null) return NotFound();
            return File(file.Stream, file.ContentType, file.FileName);
        }
    }
}
using career_sytem_recoman.Models.DTOs.Rating;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        /// <summary>
        /// إضافة تقييم لمستخدم آخر (لا يمكن تقييم النفس، والتقييم المكرر ممنوع)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            try
            {
                var result = await _ratingService.CreateRatingAsync(dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // نعيد 400 للأخطاء المتوقعة (مثل تقييم النفس أو التكرار)
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// جلب جميع التقييمات التي حصل عليها مستخدم معين
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserRatings(int userId)
        {
            try
            {
                var ratings = await _ratingService.GetRatingsForUserAsync(userId);
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                // إذا لم يوجد المستخدم نعيد 404
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}
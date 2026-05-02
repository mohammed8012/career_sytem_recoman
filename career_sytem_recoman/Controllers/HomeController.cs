using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : (int?)null;
        }

        [HttpGet("suggestions")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSuggestions()
        {
            var userId = GetCurrentUserId();
            var suggestions = await _homeService.GetSuggestionsAsync(userId);
            return Ok(suggestions);
        }
    }
}
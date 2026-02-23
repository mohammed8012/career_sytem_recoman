using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            var suggestions = await _homeService.GetSuggestionsAsync();
            return Ok(suggestions);
        }
    }
}

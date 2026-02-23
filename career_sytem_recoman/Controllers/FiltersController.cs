using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace career_sytem_recoman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltersController : ControllerBase
    {
        private readonly IFilterService _filterService;

        public FiltersController(IFilterService filterService)
        {
            _filterService = filterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilterOptions()
        {
            var options = await _filterService.GetFilterOptionsAsync();
            return Ok(options);
        }
    }
}
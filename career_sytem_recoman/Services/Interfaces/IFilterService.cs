using career_sytem_recoman.Models.DTOs.Filters;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IFilterService
    {
        Task<FilterOptionsDto> GetFilterOptionsAsync();
    }
}
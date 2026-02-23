using career_sytem_recoman.Models.DTOs.Home;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeSuggestionsDto> GetSuggestionsAsync();
    }
}
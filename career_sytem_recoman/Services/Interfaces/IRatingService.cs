using career_sytem_recoman.Models.DTOs.Rating;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IRatingService
    {
        Task<object> CreateRatingAsync(CreateRatingDto dto, int ratedByUserId);
        Task<List<UserRatingDto>> GetRatingsForUserAsync(int userId);
    }
}
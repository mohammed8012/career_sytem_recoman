using career_sytem_recoman.Models.DTOs.Rating;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class RatingService : IRatingService
    {
        private readonly JobPlatformContext _context;

        public RatingService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<object> CreateRatingAsync(CreateRatingDto dto, int ratedByUserId)
        {
            // Assuming ratings are stored in CourseTracking or a separate Ratings table
            // This is just a placeholder
            throw new NotImplementedException("Rating system not fully implemented.");
        }

        public async Task<List<UserRatingDto>> GetRatingsForUserAsync(int userId)
        {
            // Example: get ratings from CourseTracking where user is the rater? or ratee?
            // Depends on your requirements
            throw new NotImplementedException();
        }
    }
}
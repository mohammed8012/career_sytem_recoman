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
            // منع تقييم النفس
            if (dto.RatedUserId == ratedByUserId)
                throw new Exception("You cannot rate yourself.");

            // التحقق من وجود المستخدم المقيَّم
            var ratedUser = await _context.Users.FindAsync(dto.RatedUserId);
            if (ratedUser == null)
                throw new Exception("User to rate not found.");

            // التحقق من عدم وجود تقييم سابق من نفس المُقيِّم لنفس المستخدم
            var existing = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RatedByUserId == ratedByUserId && r.RatedUserId == dto.RatedUserId);
            if (existing != null)
                throw new Exception("You have already rated this user.");

            var rating = new Rating
            {
                RatedByUserId = ratedByUserId,
                RatedUserId = dto.RatedUserId,
                Value = dto.Value,
                Review = dto.Review,
                CreatedAt = DateTime.UtcNow
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return new { Message = "Rating submitted successfully." };
        }

        public async Task<List<UserRatingDto>> GetRatingsForUserAsync(int userId)
        {
            // التحقق من وجود المستخدم (اختياري)
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found."); // أو إرجاع قائمة فارغة

            var ratings = await _context.Ratings
                .Where(r => r.RatedUserId == userId)
                .Include(r => r.RatedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new UserRatingDto
                {
                    Id = r.Id,
                    Value = r.Value,
                    Review = r.Review,
                    RatedByUserId = r.RatedByUserId,
                    RatedByUserName = r.RatedByUser.FirstName + " " + r.RatedByUser.LastName,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return ratings;
        }
    }
}
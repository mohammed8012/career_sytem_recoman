using career_sytem_recoman.Models.DTOs.Rating;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services;

public class RatingService : IRatingService
{
    private readonly JobPlatformContext _context;

    public RatingService(JobPlatformContext context)
    {
        _context = context;
    }

    public async Task<object> CreateRatingAsync(CreateRatingDto dto, int ratedByUserId)
    {
        // التحقق من عدم تقييم المستخدم لنفسه
        if (dto.RatedUserId == ratedByUserId)
            throw new Exception("You cannot rate yourself.");

        // البحث عن تتبع كورس للمستخدم (أو أي علاقة أخرى) يمكن إرفاق التقييم به
        // هنا سنستخدم CourseTracking كجدول للتقييمات، ونفترض أن userId هو المقيَّم (RatedUserId)
        // و ratedByUserId هو المُقيِّم (RatedByUserId) – لكن جدول CourseTracking لا يحتوي على هذه الحقول.
        // لذا هذا الحل مؤقت وسيحتاج إلى إعادة هيكلة عند وجود جدول تقييمات منفصل.

        // كحل مؤقت، نبحث عن أي كورس تم تتبعه من قبل المستخدم المقيَّم ونضيف التقييم لأول واحد
        var courseTracking = await _context.CourseTrackings
            .FirstOrDefaultAsync(ct => ct.UserId == dto.RatedUserId);

        if (courseTracking == null)
            throw new Exception("No course tracking found for this user to rate.");

        // تحديث التقييم في CourseTracking (هذا ليس مثالياً، لكنه يعمل مؤقتاً)
        courseTracking.Rating = dto.Value;
        courseTracking.Review = dto.Review;

        await _context.SaveChangesAsync();

        return new { Message = "Rating submitted successfully." };
    }

    public async Task<List<UserRatingDto>> GetRatingsForUserAsync(int userId)
    {
        // جلب التقييمات من CourseTracking للمستخدم المطلوب
        var ratings = await _context.CourseTrackings
            .Where(ct => ct.UserId == userId && ct.Rating != null)
            .Select(ct => new UserRatingDto
            {
                Id = ct.TrackId,
                Value = ct.Rating ?? 0,
                Review = ct.Review,
                RatedByUserId = ct.UserId, // هنا المشكلة: ليس لدينا المُقيِّم
                RatedByUserName = "Unknown", // مؤقت
                CreatedAt = ct.LastAccessed ?? DateTime.UtcNow
            })
            .ToListAsync();

        return ratings;
    }
}
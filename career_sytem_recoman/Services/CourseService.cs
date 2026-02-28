using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.Saved;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services;

public class CourseService(JobPlatformContext context) : ICourseService
{
    public async Task<List<CourseDto>> GetCoursesAsync(CourseFilterDto filter)
    {
        var query = context.Courses.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(c => c.Category == filter.Category);
        if (!string.IsNullOrEmpty(filter.Provider))
            query = query.Where(c => c.Provider == filter.Provider);

        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(((filter.Page ?? 1) - 1) * (filter.PageSize ?? 10))
            .Take(filter.PageSize ?? 10)
            .ToListAsync();

        return [.. courses.Select(c => new CourseDto
        {
            CourseId = c.CourseId,
            Title = c.Title,
            Description = c.Description,
            Category = c.Category,
            Provider = c.Provider,
            ImageUrl = c.ImageUrl,
            CourseUrl = c.CourseUrl,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        })];
    }

    public async Task<CourseDto> GetCourseAsync(int courseId)
    {
        var course = await context.Courses.FindAsync(courseId);
        if (course is null)
            throw new Exception("Course not found.");

        return new CourseDto
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            Provider = course.Provider,
            ImageUrl = course.ImageUrl,
            CourseUrl = course.CourseUrl,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt
        };
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
    {
        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Provider = dto.Provider,
            ImageUrl = dto.ImageUrl,
            CourseUrl = dto.CourseUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        return await GetCourseAsync(course.CourseId);
    }

    public async Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseDto dto)
    {
        var course = await context.Courses.FindAsync(courseId);
        if (course is null)
            throw new Exception("Course not found.");

        if (!string.IsNullOrEmpty(dto.Title))
            course.Title = dto.Title;
        if (dto.Description is not null)
            course.Description = dto.Description;
        if (dto.Category is not null)
            course.Category = dto.Category;
        if (dto.Provider is not null)
            course.Provider = dto.Provider;
        if (dto.ImageUrl is not null)
            course.ImageUrl = dto.ImageUrl;
        if (dto.CourseUrl is not null)
            course.CourseUrl = dto.CourseUrl;
        if (dto.IsActive is not null)
            course.IsActive = dto.IsActive.Value;

        await context.SaveChangesAsync();
        return await GetCourseAsync(courseId);
    }

    public async Task DeleteCourseAsync(int courseId)
    {
        var course = await context.Courses.FindAsync(courseId);
        if (course is not null)
        {
            context.Courses.Remove(course);
            await context.SaveChangesAsync();
        }
    }

    public async Task<CourseTrackingDto> TrackCourseAsync(int userId, int courseId, CourseTrackingDto tracking)
    {
        var existing = await context.CourseTrackings
            .FirstOrDefaultAsync(ct => ct.UserId == userId && ct.CourseId == courseId);

        if (existing is null)
        {
            var newTracking = new CourseTracking
            {
                UserId = userId,
                CourseId = courseId,
                ProgressPercent = tracking.ProgressPercent ?? 0,
                IsCompleted = tracking.IsCompleted ?? false,
                LastAccessed = tracking.LastAccessed ?? DateTime.UtcNow,
                Rating = tracking.Rating,
                Review = tracking.Review
            };
            context.CourseTrackings.Add(newTracking);
            await context.SaveChangesAsync();

            tracking.TrackId = newTracking.TrackId;
            return tracking;
        }
        else
        {
            existing.ProgressPercent = tracking.ProgressPercent ?? existing.ProgressPercent;
            existing.IsCompleted = tracking.IsCompleted ?? existing.IsCompleted;
            existing.LastAccessed = tracking.LastAccessed ?? DateTime.UtcNow;
            existing.Rating = tracking.Rating ?? existing.Rating;
            existing.Review = tracking.Review ?? existing.Review;

            await context.SaveChangesAsync();

            tracking.TrackId = existing.TrackId;
            return tracking;
        }
    }

    public async Task<object> EnrollAsync(int courseId, int userId)
    {
        var course = await context.Courses.FindAsync(courseId);
        if (course is null)
            throw new Exception("Course not found.");

        var existing = await context.CourseTrackings
            .FirstOrDefaultAsync(ct => ct.UserId == userId && ct.CourseId == courseId);
        if (existing is not null)
            return new { Message = "Already enrolled." };

        var tracking = new CourseTracking
        {
            UserId = userId,
            CourseId = courseId,
            ProgressPercent = 0,
            IsCompleted = false,
            LastAccessed = DateTime.UtcNow
        };

        context.CourseTrackings.Add(tracking);
        await context.SaveChangesAsync();

        return new { Message = "Enrolled successfully.", TrackId = tracking.TrackId };
    }

    public async Task<object> SaveCourseAsync(int courseId, int userId)
    {
        var course = await context.Courses.FindAsync(courseId);
        if (course is null)
            throw new Exception("Course not found.");

        var existing = await context.CourseTrackings
            .FirstOrDefaultAsync(ct => ct.UserId == userId && ct.CourseId == courseId);
        if (existing is not null)
            return new { Message = "Course already saved (tracked)." };

        var tracking = new CourseTracking
        {
            UserId = userId,
            CourseId = courseId,
            ProgressPercent = 0,
            IsCompleted = false,
            LastAccessed = DateTime.UtcNow
        };

        context.CourseTrackings.Add(tracking);
        await context.SaveChangesAsync();

        return new { Message = "Course saved successfully.", TrackId = tracking.TrackId };
    }

    public async Task UnsaveCourseAsync(int courseId, int userId)
    {
        var existing = await context.CourseTrackings
            .FirstOrDefaultAsync(ct => ct.UserId == userId && ct.CourseId == courseId);
        if (existing is not null)
        {
            context.CourseTrackings.Remove(existing);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<SavedItemDto>> GetSavedCoursesAsync(int userId)
    {
        var savedCourses = await context.CourseTrackings
            .Where(ct => ct.UserId == userId)
            .Include(ct => ct.Course)
            .OrderByDescending(ct => ct.LastAccessed)
            .Select(ct => new SavedItemDto
            {
                Id = ct.CourseId,
                Title = ct.Course.Title,
                Type = "Course",
                SavedAt = ct.LastAccessed ?? ct.Course.CreatedAt ?? DateTime.UtcNow
            })
            .ToListAsync();

        return [.. savedCourses];
    }
}
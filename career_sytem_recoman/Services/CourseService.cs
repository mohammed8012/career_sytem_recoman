using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.Saved;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class CourseService : ICourseService
    {
        private readonly JobPlatformContext _context;

        public CourseService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<CourseDto>> GetCoursesAsync(CourseFilterDto filter)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(c => c.Category == filter.Category);
            if (!string.IsNullOrEmpty(filter.Provider))
                query = query.Where(c => c.Provider == filter.Provider);

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(((filter.Page ?? 1) - 1) * (filter.PageSize ?? 10))
                .Take(filter.PageSize ?? 10)
                .ToListAsync();

            return courses.Select(c => new CourseDto
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
            }).ToList();
        }

        public async Task<CourseDto> GetCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
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

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return await GetCourseAsync(course.CourseId);
        }

        public async Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new Exception("Course not found.");

            if (!string.IsNullOrEmpty(dto.Title))
                course.Title = dto.Title;
            if (dto.Description != null)
                course.Description = dto.Description;
            if (dto.Category != null)
                course.Category = dto.Category;
            if (dto.Provider != null)
                course.Provider = dto.Provider;
            if (dto.ImageUrl != null)
                course.ImageUrl = dto.ImageUrl;
            if (dto.CourseUrl != null)
                course.CourseUrl = dto.CourseUrl;
            if (dto.IsActive.HasValue)
                course.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return await GetCourseAsync(courseId);
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CourseTrackingDto> TrackCourseAsync(int userId, int courseId, CourseTrackingDto tracking)
        {
            var existing = await _context.CourseTrackings
                .FirstOrDefaultAsync(ct => ct.UserId == userId && ct.CourseId == courseId);

            if (existing == null)
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
                _context.CourseTrackings.Add(newTracking);
                await _context.SaveChangesAsync();

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

                await _context.SaveChangesAsync();

                tracking.TrackId = existing.TrackId;
                return tracking;
            }
        }

        // الدوال المطلوبة من الواجهة ولكن غير مدعومة بشكل كامل
        public Task<object> EnrollAsync(int courseId, int userId)
        {
            // يمكن استخدام TrackCourseAsync للتسجيل
            throw new NotImplementedException("Use TrackCourseAsync for enrollment.");
        }

        public Task<object> SaveCourseAsync(int courseId, int userId)
        {
            throw new NotImplementedException("Saved courses not implemented.");
        }

        public Task UnsaveCourseAsync(int courseId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<SavedItemDto>> GetSavedCoursesAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
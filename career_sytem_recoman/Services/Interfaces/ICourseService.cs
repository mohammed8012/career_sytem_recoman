using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.Saved;
namespace career_sytem_recoman.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetCoursesAsync(CourseFilterDto filter);
        Task<CourseDto> GetCourseAsync(int courseId);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
        Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseDto dto);
        Task DeleteCourseAsync(int courseId);
        Task<CourseTrackingDto> TrackCourseAsync(int userId, int courseId, CourseTrackingDto tracking);

        // الدوال الجديدة المطلوبة من الكونترولر
        Task<object> EnrollAsync(int courseId, int userId);
        Task<object> SaveCourseAsync(int courseId, int userId);
        Task UnsaveCourseAsync(int courseId, int userId);
        Task<List<SavedItemDto>> GetSavedCoursesAsync(int userId);
    }
}
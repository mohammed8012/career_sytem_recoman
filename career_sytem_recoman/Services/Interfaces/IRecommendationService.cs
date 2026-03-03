using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.Course;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<JobDto>> GetRecommendedJobsAsync(int userId);
        Task<List<CourseDto>> GetRecommendedCoursesAsync(int userId);
    }
}
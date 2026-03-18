using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.Course;

namespace career_sytem_recoman.Models.DTOs.Dashboard
{
    public class DashboardOverviewDto
    {
        public string? Analysis { get; set; }
        public List<string>? Skills { get; set; }
        public List<JobDto>? RecommendedJobs { get; set; }
        public List<CourseDto>? RecommendedCourses { get; set; }
    }
}
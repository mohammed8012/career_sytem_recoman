using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.Course;

namespace career_sytem_recoman.Models.DTOs.Home
{
    public class HomeSuggestionsDto
    {
        public List<JobDto> Jobs { get; set; } = [];
        public List<CourseDto> Courses { get; set; } = [];
    }
}
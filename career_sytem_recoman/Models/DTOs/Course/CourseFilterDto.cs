namespace career_sytem_recoman.Models.DTOs.Course
{
    public class CourseFilterDto
    {
        public string? Category { get; set; }
        public string? Provider { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
    }
}
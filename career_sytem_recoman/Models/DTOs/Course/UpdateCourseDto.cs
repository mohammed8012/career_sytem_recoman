namespace career_sytem_recoman.Models.DTOs.Course
{
    public class UpdateCourseDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Provider { get; set; }
        public string? ImageUrl { get; set; }
        public string? CourseUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
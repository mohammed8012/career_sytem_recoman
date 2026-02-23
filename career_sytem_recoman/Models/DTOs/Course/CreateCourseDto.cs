using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Course
{
    public class CreateCourseDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? Provider { get; set; }

        [Url, MaxLength(255)]
        public string? ImageUrl { get; set; }

        [Url, MaxLength(255)]
        public string? CourseUrl { get; set; }
    }
}
namespace career_sytem_recoman.Models.DTOs.Course
{
    public class CourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public string? Provider { get; set; }
        public string? CourseUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }

        // معلومات التتبع للمستخدم الحالي
        public CourseTrackingDto? Tracking { get; set; }
    }
}
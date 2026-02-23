namespace career_sytem_recoman.Models.DTOs.Course
{
    public class CourseTrackingDto
    {
        public int TrackId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int? ProgressPercent { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? LastAccessed { get; set; }
        public int? Rating { get; set; }
        public string? Review { get; set; }
    }
}
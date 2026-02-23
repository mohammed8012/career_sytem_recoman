namespace career_sytem_recoman.Models.DTOs.Job
{
    public class ApplicantDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? CvPath { get; set; }
        public string? Status { get; set; }
    }
}
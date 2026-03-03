using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Models.DTOs.Course;

namespace career_sytem_recoman.Models.DTOs.User
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string UserType { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Cvpath { get; set; }
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? Specialization { get; set; }
        public string? FieldsAvailable { get; set; }
        public int? FoundedYear { get; set; }
        public string? CompanySize { get; set; }
        public string? LogoPath { get; set; }
        public string? CvAnalysis { get; set; }
        public List<string>? SkillsList { get; set; }

        // العلاقات
        public List<ApplicationDto>? Applications { get; set; }
        public List<CourseTrackingDto>? CourseTrackings { get; set; }
    }
}
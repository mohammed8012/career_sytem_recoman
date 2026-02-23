using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.User;

namespace career_sytem_recoman.Models.DTOs.Application
{
    public class ApplicationDto
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }
        public string? InteractionType { get; set; }
        public string? Status { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? CompanyNotes { get; set; }

        // معلومات المستخدم المتقدم
        public UserProfileDto? User { get; set; }

        // معلومات الوظيفة المتقدم لها
        public JobDto? Job { get; set; }
    }
}
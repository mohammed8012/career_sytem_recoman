using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Models.DTOs.User;

namespace career_sytem_recoman.Models.DTOs.Job
{
    public class JobDto
    {
        public int JobId { get; set; }
        public int CompanyId { get; set; }
        public string JobTitle { get; set; } = null!;
        public string? JobCategory { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public int? MinExperience { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }

        // معلومات الشركة (صاحب العمل)
        public UserProfileDto? Company { get; set; }

        // قائمة المتقدمين (للاستخدام من قبل الشركة)
        public List<ApplicationDto>? Applications { get; set; }

        // عدد المتقدمين (محسوب)
        public int ApplicantsCount => Applications?.Count ?? 0;
    }
}
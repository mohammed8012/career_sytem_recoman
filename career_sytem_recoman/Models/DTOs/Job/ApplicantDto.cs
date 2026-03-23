namespace career_sytem_recoman.Models.DTOs.Job
{
    public class ApplicantDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }               // إضافة
        public int? YearsOfExperience { get; set; }        // إضافة
        public string? Bio { get; set; }                   // إضافة
        public DateTime AppliedAt { get; set; }
        public string? CvPath { get; set; }
        public string? Status { get; set; }
        // النسبة المئوية للمطابقة بين مهارات المتقدم ومتطلبات الوظيفة
        public double MatchScore { get; set; } = 0;
        public List<string>? SkillsList { get; set; }   // 👈 يجب وجود هذا السطر
    }
}
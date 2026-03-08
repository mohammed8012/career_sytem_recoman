namespace career_sytem_recoman.Models.DTOs.User
{
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
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
        // الحقول الجديدة للصور
        public string? ProfileImagePath { get; set; }
        public string? CoverImagePath { get; set; }
        // الحقول الخاصة بتحليل السيرة الذاتية
        public string? CvAnalysis { get; set; }
        public List<string>? SkillsList { get; set; }
    }
}
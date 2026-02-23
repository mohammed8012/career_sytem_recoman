using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Phone, MaxLength(20)]
        public string? Phone { get; set; }

        [Required, MaxLength(20)]
        public string UserType { get; set; } // "JobSeeker" or "Employer"

        // حقول خاصة بالباحث عن عمل
        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? Skills { get; set; } // يمكن تخزينها كنص مفصول بفواصل

        [Range(0, 50)]
        public int? YearsOfExperience { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        // حقول خاصة بالشركة
        [MaxLength(100)]
        public string? CompanyName { get; set; }

        [MaxLength(200)]
        public string? CompanyAddress { get; set; }

        [MaxLength(100)]
        public string? FieldsAvailable { get; set; }

        [Range(1800, 2100)]
        public int? FoundedYear { get; set; }

        [MaxLength(20)]
        public string? CompanySize { get; set; }

        [Url, MaxLength(255)]
        public string? LogoPath { get; set; }
    }
}
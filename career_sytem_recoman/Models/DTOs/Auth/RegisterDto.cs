using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required, MaxLength(20)]
        public string? UserType { get; set; } // ملحوظة: سيكون مطلوبًا لكن بدون قيمة افتراضية، مع علامة استفهام لأنها قد تكون null قبل التحقق

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? Skills { get; set; }

        [Range(0, 50)]
        public int? YearsOfExperience { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

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

        [MaxLength(255)]
        public string? LogoPath { get; set; }
    }
}
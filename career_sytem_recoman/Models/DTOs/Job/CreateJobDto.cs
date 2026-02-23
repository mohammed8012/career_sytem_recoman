using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Job
{
    public class CreateJobDto
    {
        [Required, MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? JobCategory { get; set; }

        public string? Description { get; set; }

        public string? Requirements { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(20)]
        public string? JobType { get; set; }

        public int? MinExperience { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        [Required]
        public int CompanyId { get; set; }
    }
}
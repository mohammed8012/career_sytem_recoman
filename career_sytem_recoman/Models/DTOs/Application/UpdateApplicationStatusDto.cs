using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Application
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = null!; // مثلاً: "Accepted", "Rejected", "UnderReview"
    }
}
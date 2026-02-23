using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Application
{
    public class CreateApplicationDto
    {
        [Required]
        public int JobId { get; set; }

        public string? CompanyNotes { get; set; } // يمكن استخدامه كـ Cover Letter
    }
}
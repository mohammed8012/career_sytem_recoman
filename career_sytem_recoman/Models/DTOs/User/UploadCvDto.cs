using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.User
{
    public class UploadCvDto
    {
        [Required]
        public IFormFile CvFile { get; set; } = null!;
    }
}
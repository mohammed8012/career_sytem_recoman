using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.User
{
    public class UploadImageDto
    {
        [Required]
        public IFormFile ImageFile { get; set; } = null!;
    }
}
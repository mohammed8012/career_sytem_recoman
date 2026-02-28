using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Auth
{
    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
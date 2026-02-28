using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
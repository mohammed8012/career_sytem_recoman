using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Auth
{
    public class SocialLoginDto
    {
        [Required]
        public string Provider { get; set; } // "Google", "Facebook"

        [Required]
        public string Token { get; set; }
    }
}
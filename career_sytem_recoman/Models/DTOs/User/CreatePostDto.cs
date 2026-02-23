using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.User
{
    public class CreatePostDto
    {
        [Required, MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
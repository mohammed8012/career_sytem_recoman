using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Posts
{
    public class CreatePostDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
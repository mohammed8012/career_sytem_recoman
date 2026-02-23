using System.ComponentModel.DataAnnotations;

namespace career_sytem_recoman.Models.DTOs.Rating
{
    public class CreateRatingDto
    {
        [Required, Range(1, 5)]
        public int Value { get; set; }

        [MaxLength(500)]
        public string? Review { get; set; }

        [Required]
        public int RatedUserId { get; set; }
    }
}
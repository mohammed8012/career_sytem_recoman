namespace career_sytem_recoman.Models.DTOs.Rating
{
    public class UserRatingDto
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public string? Review { get; set; }
        public int RatedByUserId { get; set; }
        public string RatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
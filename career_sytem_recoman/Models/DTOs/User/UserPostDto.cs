namespace career_sytem_recoman.Models.DTOs.User
{
    public class UserPostDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
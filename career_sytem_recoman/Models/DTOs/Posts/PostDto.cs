namespace career_sytem_recoman.Models.DTOs.Posts
{
    public class PostDto
    {
        public int PostId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
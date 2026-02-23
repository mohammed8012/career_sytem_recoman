namespace career_sytem_recoman.Models.DTOs.Saved
{
    public class SavedItemDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }      // nullable لتجنب التحذيرات
        public string? Type { get; set; }       // nullable لتجنب التحذيرات
        public DateTime SavedAt { get; set; }
    }
}
using System;

namespace career_sytem_recoman.Models.DTOs.Saved
{
    public class SavedJobDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
    }
}
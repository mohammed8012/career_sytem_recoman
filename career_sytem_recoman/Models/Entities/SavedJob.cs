using System;

namespace career_sytem_recoman.Models.Entities
{
    public class SavedJob
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }
        public DateTime SavedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Job Job { get; set; } = null!;
    }
}
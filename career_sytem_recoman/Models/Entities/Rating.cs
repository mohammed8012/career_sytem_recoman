using System;

namespace career_sytem_recoman.Models.Entities
{
    public partial class Rating
    {
        public int Id { get; set; }
        public int RatedByUserId { get; set; }
        public int RatedUserId { get; set; }
        public int Value { get; set; }
        public string? Review { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User RatedByUser { get; set; } = null!;
        public virtual User RatedUser { get; set; } = null!;
    }
}
using System;

namespace career_sytem_recoman.Models.Entities
{
    public partial class Post
    {
        public int PostId { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual User Company { get; set; } = null!;
    }
}
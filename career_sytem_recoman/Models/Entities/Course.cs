using System;
using System.Collections.Generic;

namespace career_sytem_recoman.Models.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? ImageUrl { get; set; }

    public string? Provider { get; set; }

    public string? CourseUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<CourseTracking> CourseTrackings { get; set; } = [];
}
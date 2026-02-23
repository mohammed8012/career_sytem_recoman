using System;
using System.Collections.Generic;

namespace career_sytem_recoman.Models.Entities;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int UserId { get; set; }

    public int JobId { get; set; }

    public string? InteractionType { get; set; }

    public string? Status { get; set; }

    public DateTime? AppliedAt { get; set; }

    public string? CompanyNotes { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

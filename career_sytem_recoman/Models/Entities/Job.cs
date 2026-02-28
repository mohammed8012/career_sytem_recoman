using System;
using System.Collections.Generic;

namespace career_sytem_recoman.Models.Entities;

public partial class Job
{
    public int JobId { get; set; }

    public int CompanyId { get; set; }

    public string JobTitle { get; set; } = null!;

    public string? JobCategory { get; set; }

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public string? Location { get; set; }

    public string? JobType { get; set; }

    public int? MinExperience { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = [];

    public virtual User Company { get; set; } = null!;
}
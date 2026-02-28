using System;
using System.Collections.Generic;

namespace career_sytem_recoman.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string UserType { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Location { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Cvpath { get; set; }

    public string? Bio { get; set; }

    public string? Skills { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? CompanyName { get; set; }

    public string? CompanyAddress { get; set; }

    public string? Specialization { get; set; }

    public string? FieldsAvailable { get; set; }

    public int? FoundedYear { get; set; }

    public string? CompanySize { get; set; }

    public string? LogoPath { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = [];

    public virtual ICollection<CourseTracking> CourseTrackings { get; set; } = [];

    public virtual ICollection<Job> Jobs { get; set; } = [];
}
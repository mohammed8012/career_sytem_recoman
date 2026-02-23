namespace career_sytem_recoman.Models.DTOs.Job
{
    public class UpdateJobDto
    {
        public string? JobTitle { get; set; }
        public string? JobCategory { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public int? MinExperience { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }
}
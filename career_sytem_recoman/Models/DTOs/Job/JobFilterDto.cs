namespace career_sytem_recoman.Models.DTOs.Job
{
    public class JobFilterDto
    {
        public string? JobCategory { get; set; }
        public string? JobType { get; set; }
        public string? Location { get; set; }
        public int? MinExperience { get; set; }
        public int? CompanyId { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
    }
}
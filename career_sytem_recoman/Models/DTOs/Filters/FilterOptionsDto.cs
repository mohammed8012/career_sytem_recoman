namespace career_sytem_recoman.Models.DTOs.Filters
{
    public class FilterOptionsDto
    {
        public List<string> JobCategories { get; set; } = new();
        public List<string> JobTypes { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<string> CourseCategories { get; set; } = new();
        public List<string> CourseProviders { get; set; } = new();
    }
}
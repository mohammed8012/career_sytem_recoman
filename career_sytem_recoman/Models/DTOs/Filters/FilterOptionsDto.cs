namespace career_sytem_recoman.Models.DTOs.Filters
{
    public class FilterOptionsDto
    {
        public List<string> JobCategories { get; set; } = [];
        public List<string> JobTypes { get; set; } = [];
        public List<string> Locations { get; set; } = [];
        public List<string> CourseCategories { get; set; } = [];
        public List<string> CourseProviders { get; set; } = [];
    }
}
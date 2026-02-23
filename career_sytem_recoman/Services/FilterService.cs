using career_sytem_recoman.Models.DTOs.Filters;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class FilterService : IFilterService
    {
        private readonly JobPlatformContext _context;

        public FilterService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<FilterOptionsDto> GetFilterOptionsAsync()
        {
            var jobCategories = await _context.Jobs
                .Where(j => j.JobCategory != null)
                .Select(j => j.JobCategory)
                .Distinct()
                .ToListAsync();

            var jobTypes = await _context.Jobs
                .Where(j => j.JobType != null)
                .Select(j => j.JobType)
                .Distinct()
                .ToListAsync();

            var locations = await _context.Jobs
                .Where(j => j.Location != null)
                .Select(j => j.Location)
                .Distinct()
                .ToListAsync();

            var courseCategories = await _context.Courses
                .Where(c => c.Category != null)
                .Select(c => c.Category)
                .Distinct()
                .ToListAsync();

            var courseProviders = await _context.Courses
                .Where(c => c.Provider != null)
                .Select(c => c.Provider)
                .Distinct()
                .ToListAsync();

            return new FilterOptionsDto
            {
                JobCategories = jobCategories!,
                JobTypes = jobTypes!,
                Locations = locations!,
                CourseCategories = courseCategories!,
                CourseProviders = courseProviders!
            };
        }
    }
}
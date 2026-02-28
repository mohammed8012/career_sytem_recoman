using career_sytem_recoman.Models.DTOs.Home;
using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services;

public class HomeService(JobPlatformContext context) : IHomeService
{
    public async Task<HomeSuggestionsDto> GetSuggestionsAsync()
    {
        var recentJobs = await context.Jobs
            .Where(j => j.IsActive == true)
            .OrderByDescending(j => j.CreatedAt)
            .Take(5)
            .Select(j => new JobDto
            {
                JobId = j.JobId,
                JobTitle = j.JobTitle,
                JobCategory = j.JobCategory,
                Location = j.Location,
                JobType = j.JobType,
                CompanyId = j.CompanyId
            })
            .ToListAsync();

        var recentCourses = await context.Courses
            .Where(c => c.IsActive == true)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Category = c.Category,
                Provider = c.Provider,
                ImageUrl = c.ImageUrl
            })
            .ToListAsync();

        return new HomeSuggestionsDto
        {
            Jobs = recentJobs,
            Courses = recentCourses
        };
    }
}
using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.Home;
using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services;

public class HomeService(IUserService userService, JobPlatformContext context) : IHomeService
{
    private readonly JobPlatformContext _context = context;
    private readonly IUserService _userService = userService;

    public async Task<HomeSuggestionsDto> GetSuggestionsAsync(int? userId = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var jobs = await _context.Jobs
            .Where(j => j.IsActive == true && (j.ExpiryDate == null || j.ExpiryDate > today))
            .Include(j => j.Company)
            .OrderByDescending(j => j.CreatedAt)
            .Take(5)
            .Select(j => new JobDto
            {
                JobId = j.JobId,
                CompanyId = j.CompanyId,
                JobTitle = j.JobTitle,
                JobCategory = j.JobCategory,
                Description = j.Description,
                Requirements = j.Requirements,
                Location = j.Location,
                JobType = j.JobType,
                MinExperience = j.MinExperience,
                CreatedAt = j.CreatedAt,
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyName = j.Company.CompanyName
            })
            .ToListAsync();

        var courses = await _context.Courses
            .Where(c => c.IsActive == true)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                ImageUrl = c.ImageUrl,
                Provider = c.Provider,
                CourseUrl = c.CourseUrl,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive
            })
            .ToListAsync();

        if (userId.HasValue && userId.Value > 0)
        {
            var user = await _userService.GetProfileAsync(userId.Value);
            var userSkills = user.SkillsList ?? [];

            if (userSkills.Count > 0)
            {
                foreach (var job in jobs)
                    job.MatchScore = CalculateMatchScore(job, userSkills);
                foreach (var course in courses)
                    course.MatchScore = CalculateCourseMatchScore(course, userSkills);
            }
        }

        return new HomeSuggestionsDto { Jobs = jobs, Courses = courses };
    }

    private static double CalculateMatchScore(JobDto job, List<string> userSkills)
    {
        var textToSearch = (job.JobTitle + " " + job.Description + " " + job.Requirements).ToLower();
        int matchCount = userSkills.Count(skill => textToSearch.Contains(skill, StringComparison.OrdinalIgnoreCase));
        return Math.Round((matchCount / (double)userSkills.Count) * 100, 0);
    }

    private static double CalculateCourseMatchScore(CourseDto course, List<string> userSkills)
    {
        var textToSearch = (course.Title + " " + course.Description).ToLower();
        int matchCount = userSkills.Count(skill => textToSearch.Contains(skill, StringComparison.OrdinalIgnoreCase));
        return Math.Round((matchCount / (double)userSkills.Count) * 100, 0);
    }
}
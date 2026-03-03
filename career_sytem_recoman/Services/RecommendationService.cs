using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace career_sytem_recoman.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly JobPlatformContext _context;
        private readonly IUserService _userService;
        private readonly IJobService _jobService;
        private readonly ICourseService _courseService;

        public RecommendationService(
            JobPlatformContext context,
            IUserService userService,
            IJobService jobService,
            ICourseService courseService)
        {
            _context = context;
            _userService = userService;
            _jobService = jobService;
            _courseService = courseService;
        }

        public async Task<List<JobDto>> GetRecommendedJobsAsync(int userId)
        {
            var user = await _userService.GetProfileAsync(userId);
            var userSkills = user.SkillsList ?? new List<string>();

            if (userSkills.Count == 0)
                return new List<JobDto>();

            var allJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = 100 }); // جلب عدد كبير للتصفية

            var scoredJobs = allJobs
                .Select(job => new
                {
                    Job = job,
                    Score = CalculateJobMatchScore(job, userSkills)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Job)
                .ToList();

            return scoredJobs;
        }

        public async Task<List<CourseDto>> GetRecommendedCoursesAsync(int userId)
        {
            var user = await _userService.GetProfileAsync(userId);
            var userSkills = user.SkillsList ?? new List<string>();

            if (userSkills.Count == 0)
                return new List<CourseDto>();

            var allCourses = await _courseService.GetCoursesAsync(new CourseFilterDto { PageSize = 100 });

            var scoredCourses = allCourses
                .Select(course => new
                {
                    Course = course,
                    Score = CalculateCourseMatchScore(course, userSkills)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Course)
                .ToList();

            return scoredCourses;
        }

        private int CalculateJobMatchScore(JobDto job, List<string> userSkills)
        {
            if (job.JobTitle == null && job.Description == null && job.Requirements == null)
                return 0;

            var textToSearch = (job.JobTitle + " " + job.Description + " " + job.Requirements).ToLower();
            return userSkills.Count(skill => textToSearch.Contains(skill.ToLower()));
        }

        private int CalculateCourseMatchScore(CourseDto course, List<string> userSkills)
        {
            if (course.Title == null && course.Description == null)
                return 0;

            var textToSearch = (course.Title + " " + course.Description).ToLower();
            return userSkills.Count(skill => textToSearch.Contains(skill.ToLower()));
        }
    }
}
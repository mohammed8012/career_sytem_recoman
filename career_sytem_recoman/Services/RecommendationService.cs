using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// جلب قائمة مهارات موحدة للمستخدم من كلا الحقلين: SkillsList (JSON) و Skills (نص قديم)
        /// </summary>
        private async Task<List<string>> GetCombinedSkillsAsync(int userId)
        {
            var user = await _userService.GetProfileAsync(userId);
            var skillsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // إضافة مهارات من SkillsList (المصفوفة الجديدة)
            if (user.SkillsList != null && user.SkillsList.Any())
            {
                foreach (var skill in user.SkillsList)
                    skillsSet.Add(skill.Trim());
            }

            // إضافة مهارات من Skills (النص القديم) - نقسم على فواصل أو مسافات أو أسطر
            if (!string.IsNullOrWhiteSpace(user.Skills))
            {
                // تقسيم النص على فواصل، مسافات، أسطر جديدة، أو علامات ترقيم
                var rawSkills = Regex.Split(user.Skills, @"[,\n\r\t]+")
                                     .Select(s => s.Trim())
                                     .Where(s => s.Length > 0 && s.Length < 50);
                foreach (var skill in rawSkills)
                    skillsSet.Add(skill);
            }

            return skillsSet.ToList();
        }

        public async Task<List<JobDto>> GetRecommendedJobsAsync(int userId)
        {
            var userSkills = await GetCombinedSkillsAsync(userId);

            // إذا لم تكن هناك مهارات، نرجع آخر 5 وظائف نشطة
            if (userSkills.Count == 0)
            {
                var defaultJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = 5 });
                return defaultJobs;
            }

            var allJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = 100 });

            var scoredJobs = allJobs
                .Select(job => new
                {
                    Job = job,
                    Score = CalculateJobMatchScore(job, userSkills)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => new JobDto
                {
                    JobId = x.Job.JobId,
                    CompanyId = x.Job.CompanyId,
                    JobTitle = x.Job.JobTitle,
                    JobCategory = x.Job.JobCategory,
                    Description = x.Job.Description,
                    Requirements = x.Job.Requirements,
                    Location = x.Job.Location,
                    JobType = x.Job.JobType,
                    MinExperience = x.Job.MinExperience,
                    CreatedAt = x.Job.CreatedAt,
                    ExpiryDate = x.Job.ExpiryDate,
                    IsActive = x.Job.IsActive,
                    MatchScore = Math.Round((x.Score / (double)userSkills.Count) * 100, 0)
                })
                .ToList();

            if (scoredJobs.Count == 0)
            {
                var defaultJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = 5 });
                return defaultJobs;
            }

            return scoredJobs;
        }

        public async Task<List<CourseDto>> GetRecommendedCoursesAsync(int userId)
        {
            var userSkills = await GetCombinedSkillsAsync(userId);

            if (userSkills.Count == 0)
            {
                var defaultCourses = await _courseService.GetCoursesAsync(new CourseFilterDto { PageSize = 5 });
                return defaultCourses;
            }

            var allCourses = await _courseService.GetCoursesAsync(new CourseFilterDto { PageSize = 100 });

            var scoredCourses = allCourses
                .Select(course => new
                {
                    Course = course,
                    Score = CalculateCourseMatchScore(course, userSkills)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => new CourseDto
                {
                    CourseId = x.Course.CourseId,
                    Title = x.Course.Title,
                    Description = x.Course.Description,
                    Category = x.Course.Category,
                    ImageUrl = x.Course.ImageUrl,
                    Provider = x.Course.Provider,
                    CourseUrl = x.Course.CourseUrl,
                    CreatedAt = x.Course.CreatedAt,
                    IsActive = x.Course.IsActive,
                    MatchScore = Math.Round((x.Score / (double)userSkills.Count) * 100, 0)
                })
                .ToList();

            if (scoredCourses.Count == 0)
            {
                var defaultCourses = await _courseService.GetCoursesAsync(new CourseFilterDto { PageSize = 5 });
                return defaultCourses;
            }

            return scoredCourses;
        }

        private int CalculateJobMatchScore(JobDto job, List<string> userSkills)
        {
            var textToSearch = (job.JobTitle + " " + job.Description + " " + job.Requirements).ToLower();
            return userSkills.Count(skill => textToSearch.Contains(skill.ToLower()));
        }

        private int CalculateCourseMatchScore(CourseDto course, List<string> userSkills)
        {
            var textToSearch = (course.Title + " " + course.Description).ToLower();
            return userSkills.Count(skill => textToSearch.Contains(skill.ToLower()));
        }
    }
}
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
        /// جلب النص الكامل للمستخدم للمقارنة (تحليل السيرة + المهارات + الوصف الوظيفي)
        /// </summary>
        private async Task<string> GetUserFullTextAsync(int userId)
        {
            var user = await _userService.GetProfileAsync(userId);
            var fullText = new List<string>();

            // 1. نص التحليل الكامل (CvAnalysis)
            if (!string.IsNullOrWhiteSpace(user.CvAnalysis))
                fullText.Add(user.CvAnalysis);

            // 2. الوصف الوظيفي (JobDescription)
            if (!string.IsNullOrWhiteSpace(user.JobDescription))
                fullText.Add(user.JobDescription);

            // 3. المهارات (SkillsList) كنص
            if (user.SkillsList != null && user.SkillsList.Any())
                fullText.Add(string.Join(" ", user.SkillsList));

            // 4. الحقول النصية الأخرى (Bio, Skills القديم) اختيارياً
            if (!string.IsNullOrWhiteSpace(user.Bio))
                fullText.Add(user.Bio);
            if (!string.IsNullOrWhiteSpace(user.Skills))
                fullText.Add(user.Skills);

            return string.Join(" ", fullText).ToLower();
        }

        public async Task<List<JobDto>> GetRecommendedJobsAsync(int userId)
        {
            var userFullText = await GetUserFullTextAsync(userId);

            // إذا كان النص فارغاً، نرجع آخر 5 وظائف نشطة
            if (string.IsNullOrWhiteSpace(userFullText))
            {
                var defaultJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = 5 });
                return defaultJobs;
            }

            var allJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = 100 });

            var scoredJobs = allJobs
                .Select(job => new
                {
                    Job = job,
                    Score = CalculateJobMatchScore(job, userFullText)
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
                    Company = x.Job.Company,
                    Applications = x.Job.Applications,
                    MatchScore = x.Score // Score هنا يمثل النسبة المئوية
                })
                .ToList();

            return scoredJobs;
        }

        public async Task<List<CourseDto>> GetRecommendedCoursesAsync(int userId)
        {
            var userFullText = await GetUserFullTextAsync(userId);

            if (string.IsNullOrWhiteSpace(userFullText))
            {
                var defaultCourses = await _courseService.GetCoursesAsync(new CourseFilterDto { PageSize = 5 });
                return defaultCourses;
            }

            var allCourses = await _courseService.GetCoursesAsync(new CourseFilterDto { PageSize = 100 });

            var scoredCourses = allCourses
                .Select(course => new
                {
                    Course = course,
                    Score = CalculateCourseMatchScore(course, userFullText)
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
                    Tracking = x.Course.Tracking,
                    MatchScore = x.Score
                })
                .ToList();

            return scoredCourses;
        }

        /// <summary>
        /// حساب درجة المطابقة بين نص المستخدم ونص الوظيفة
        /// </summary>
        private double CalculateJobMatchScore(JobDto job, string userFullText)
        {
            if (string.IsNullOrWhiteSpace(userFullText))
                return 0;

            var jobText = (job.JobTitle + " " + job.Description + " " + job.Requirements).ToLower();
            if (string.IsNullOrWhiteSpace(jobText))
                return 0;

            // استخدام TF-IDF بسيط: حساب عدد الكلمات المشتركة كنسبة
            var userWords = userFullText.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(w => w.Trim())
                                        .Where(w => w.Length > 2)
                                        .ToHashSet();

            var jobWords = jobText.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(w => w.Trim())
                                  .Where(w => w.Length > 2)
                                  .ToHashSet();

            if (userWords.Count == 0 || jobWords.Count == 0)
                return 0;

            int commonCount = userWords.Count(w => jobWords.Contains(w));
            double score = (commonCount / (double)Math.Max(userWords.Count, jobWords.Count)) * 100;
            return Math.Round(score, 0);
        }

        /// <summary>
        /// حساب درجة المطابقة بين نص المستخدم ونص الكورس
        /// </summary>
        private double CalculateCourseMatchScore(CourseDto course, string userFullText)
        {
            if (string.IsNullOrWhiteSpace(userFullText))
                return 0;

            var courseText = (course.Title + " " + course.Description).ToLower();
            if (string.IsNullOrWhiteSpace(courseText))
                return 0;

            var userWords = userFullText.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(w => w.Trim())
                                        .Where(w => w.Length > 2)
                                        .ToHashSet();

            var courseWords = courseText.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(w => w.Trim())
                                        .Where(w => w.Length > 2)
                                        .ToHashSet();

            if (userWords.Count == 0 || courseWords.Count == 0)
                return 0;

            int commonCount = userWords.Count(w => courseWords.Contains(w));
            double score = (commonCount / (double)Math.Max(userWords.Count, courseWords.Count)) * 100;
            return Math.Round(score, 0);
        }
    }
}
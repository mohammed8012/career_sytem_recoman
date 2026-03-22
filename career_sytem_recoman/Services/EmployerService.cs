using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json; // 👈 أضف هذا السطر

namespace career_sytem_recoman.Services;

public class EmployerService(JobPlatformContext context, IWebHostEnvironment env) : IEmployerService
{
    private readonly JobPlatformContext _context = context;
    private readonly IWebHostEnvironment _env = env;

    public async Task<List<JobDto>> GetJobsByEmployerAsync(int employerId)
    {
        var jobs = await _context.Jobs
            .Where(j => j.CompanyId == employerId)
            .OrderByDescending(j => j.CreatedAt)
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
                IsActive = j.IsActive
            })
            .ToListAsync();

        return jobs;
    }

    public async Task<List<ApplicantDto>> GetApplicantsForJobAsync(int jobId, int employerId)
    {
        // التحقق من أن الوظيفة تخص صاحب العمل
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null || job.CompanyId != employerId)
            throw new UnauthorizedAccessException();

        // جلب المتقدمين مع بياناتهم الكاملة
        var applications = await _context.Applications
            .Where(a => a.JobId == jobId)
            .Include(a => a.User)
            .ToListAsync();

        // نص الوظيفة للمقارنة (العنوان، الوصف، المتطلبات)
        var jobText = (job.JobTitle + " " + job.Description + " " + job.Requirements).ToLower();

        var applicants = new List<ApplicantDto>();

        foreach (var app in applications)
        {
            var user = app.User;

            // استخراج مهارات المستخدم من SkillsList (JSON array)
            List<string> userSkills = new List<string>();
            if (!string.IsNullOrEmpty(user.SkillsList))
            {
                try
                {
                    userSkills = JsonSerializer.Deserialize<List<string>>(user.SkillsList) ?? new List<string>();
                }
                catch { }
            }

            // حساب عدد المهارات المشتركة
            int matchCount = userSkills.Count(skill => jobText.Contains(skill.ToLower()));

            // حساب النسبة المئوية (إذا كان لدى المستخدم مهارات)
            double matchScore = userSkills.Any()
                ? Math.Round((matchCount / (double)userSkills.Count) * 100, 0)
                : 0;

            applicants.Add(new ApplicantDto
            {
                UserId = user.UserId,
                FullName = (user.FirstName + " " + user.LastName).Trim(),
                Email = user.Email,
                Phone = user.Phone,
                AppliedAt = app.AppliedAt ?? DateTime.UtcNow,
                CvPath = user.Cvpath,
                Status = app.Status,
                MatchScore = matchScore
            });
        }

        return applicants;
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> GetApplicantCvAsync(int applicantId, int employerId)
    {
        var hasAccess = await _context.Applications
            .Where(a => a.UserId == applicantId)
            .Join(_context.Jobs.Where(j => j.CompanyId == employerId),
                  a => a.JobId,
                  j => j.JobId,
                  (a, j) => a)
            .AnyAsync();

        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have permission to view this CV.");

        var user = await _context.Users.FindAsync(applicantId);
        if (user == null || string.IsNullOrEmpty(user.Cvpath))
            throw new FileNotFoundException("CV not found for this applicant.");

        var filePath = Path.Combine(_env.WebRootPath, user.Cvpath.TrimStart('/'));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("CV file does not exist on server.");

        var stream = File.OpenRead(filePath);
        var contentType = string.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var fileName = Path.GetFileName(filePath);

        return (stream, contentType, fileName);
    }
}
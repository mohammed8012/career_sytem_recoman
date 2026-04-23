using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace career_sytem_recoman.Services;

public class EmployerService : IEmployerService
{
    private readonly JobPlatformContext _context;
    private readonly IWebHostEnvironment _env;

    public EmployerService(JobPlatformContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

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
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null || job.CompanyId != employerId)
            throw new UnauthorizedAccessException();

        var applicants = await _context.Applications
            .Where(a => a.JobId == jobId)
            .Include(a => a.User)
            .ToListAsync();

        var result = new List<ApplicantDto>();
        foreach (var app in applicants)
        {
            var user = app.User;

            // دمج المهارات من SkillsList و Skills
            var skillsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(user.SkillsList))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(user.SkillsList);
                    if (list != null)
                        foreach (var s in list) skillsSet.Add(s.Trim());
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(user.Skills))
            {
                var rawSkills = Regex.Split(user.Skills, @"[,\n\r\t]+")
                                     .Select(s => s.Trim())
                                     .Where(s => s.Length > 0 && s.Length < 50);
                foreach (var s in rawSkills) skillsSet.Add(s);
            }
            var skills = skillsSet.ToList();

            // حساب نسبة المطابقة
            double matchScore = 0;
            if (skills.Any())
            {
                var jobText = (job.JobTitle + " " + job.Description + " " + job.Requirements).ToLower();
                int matchCount = skills.Count(skill => jobText.Contains(skill.ToLower()));
                matchScore = Math.Round((matchCount / (double)skills.Count) * 100, 0);
            }

            result.Add(new ApplicantDto
            {
                UserId = user.UserId,
                FullName = (user.FirstName + " " + user.LastName).Trim(),
                Email = user.Email,
                Phone = user.Phone,
                Location = user.Location,
                YearsOfExperience = user.YearsOfExperience,
                Bio = user.Bio,
                AppliedAt = app.AppliedAt ?? DateTime.UtcNow,
                CvPath = user.Cvpath,
                Status = app.Status,
                MatchScore = matchScore,
                SkillsList = skills
            });
        }

        // ترتيب المتقدمين تنازلياً حسب نسبة المطابقة (الأعلى أولاً)
        return result.OrderByDescending(a => a.MatchScore).ToList();
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
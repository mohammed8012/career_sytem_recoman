using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace career_sytem_recoman.Services
{
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
                .Select(a => new ApplicantDto
                {
                    UserId = a.UserId,
                    FullName = a.User.FirstName + " " + a.User.LastName,
                    Email = a.User.Email,
                    Phone = a.User.Phone,
                    AppliedAt = a.AppliedAt ?? DateTime.UtcNow,
                    CvPath = a.User.Cvpath,
                    Status = a.Status
                })
                .ToListAsync();

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
            var contentType = Path.GetExtension(filePath).ToLower() == ".pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            var fileName = Path.GetFileName(filePath);

            return (stream, contentType, fileName);
        }
    }
}
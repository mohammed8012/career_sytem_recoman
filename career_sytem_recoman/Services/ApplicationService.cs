using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.User;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly JobPlatformContext _context;

        public ApplicationService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<ApplicationDto> ApplyAsync(int userId, CreateApplicationDto dto)
        {
            // Check if already applied
            var existing = await _context.Applications
                .FirstOrDefaultAsync(a => a.UserId == userId && a.JobId == dto.JobId);
            if (existing != null)
                throw new Exception("You have already applied for this job.");

            var application = new Application
            {
                UserId = userId,
                JobId = dto.JobId,
                CompanyNotes = dto.CompanyNotes,
                InteractionType = "Applied",
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return await GetApplicationAsync(application.ApplicationId);
        }

        public async Task<ApplicationDto> GetApplicationAsync(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                throw new Exception("Application not found.");

            return new ApplicationDto
            {
                ApplicationId = application.ApplicationId,
                UserId = application.UserId,
                JobId = application.JobId,
                InteractionType = application.InteractionType,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                CompanyNotes = application.CompanyNotes,
                User = new UserProfileDto
                {
                    UserId = application.User.UserId,
                    FirstName = application.User.FirstName,
                    LastName = application.User.LastName,
                    Email = application.User.Email,
                    Phone = application.User.Phone,
                    Cvpath = application.User.Cvpath
                },
                Job = new JobDto
                {
                    JobId = application.Job.JobId,
                    JobTitle = application.Job.JobTitle,
                    JobCategory = application.Job.JobCategory,
                    Location = application.Job.Location,
                    JobType = application.Job.JobType
                }
            };
        }

        public async Task<List<ApplicationDto>> GetUserApplicationsAsync(int userId)
        {
            var applications = await _context.Applications
                .Where(a => a.UserId == userId)
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return applications.Select(a => new ApplicationDto
            {
                ApplicationId = a.ApplicationId,
                UserId = a.UserId,
                JobId = a.JobId,
                InteractionType = a.InteractionType,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                CompanyNotes = a.CompanyNotes,
                Job = new JobDto
                {
                    JobId = a.Job.JobId,
                    JobTitle = a.Job.JobTitle,
                    JobCategory = a.Job.JobCategory,
                    Location = a.Job.Location,
                    JobType = a.Job.JobType
                }
            }).ToList();
        }

        public async Task<List<ApplicationDto>> GetJobApplicationsAsync(int jobId, int employerId)
        {
            // Check if the job belongs to the employer
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null || job.CompanyId != employerId)
                throw new UnauthorizedAccessException();

            var applications = await _context.Applications
                .Where(a => a.JobId == jobId)
                .Include(a => a.User)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return applications.Select(a => new ApplicationDto
            {
                ApplicationId = a.ApplicationId,
                UserId = a.UserId,
                JobId = a.JobId,
                InteractionType = a.InteractionType,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                CompanyNotes = a.CompanyNotes,
                User = new UserProfileDto
                {
                    UserId = a.User.UserId,
                    FirstName = a.User.FirstName,
                    LastName = a.User.LastName,
                    Email = a.User.Email,
                    Phone = a.User.Phone,
                    Cvpath = a.User.Cvpath
                }
            }).ToList();
        }

        public async Task<ApplicationDto> UpdateApplicationStatusAsync(int applicationId, UpdateApplicationStatusDto dto, int employerId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                throw new Exception("Application not found.");

            // Check if the employer owns the job
            if (application.Job.CompanyId != employerId)
                throw new UnauthorizedAccessException();

            application.Status = dto.Status;
            await _context.SaveChangesAsync();

            return await GetApplicationAsync(applicationId);
        }
    }
}
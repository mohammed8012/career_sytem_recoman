using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.DTOs.User;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace career_sytem_recoman.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly JobPlatformContext _context;
        private readonly INotificationService _notificationService;

        public ApplicationService(JobPlatformContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ApplicationDto> ApplyAsync(int userId, CreateApplicationDto dto)
        {
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
                    Cvpath = a.User.Cvpath,
                    Location = a.User.Location,                         // ✅ إضافة
                    YearsOfExperience = a.User.YearsOfExperience,       // ✅ إضافة
                    Bio = a.User.Bio,                                   // ✅ إضافة
                    SkillsList = !string.IsNullOrEmpty(a.User.SkillsList)
                        ? JsonSerializer.Deserialize<List<string>>(a.User.SkillsList)
                        : new List<string>()
                }
            }).ToList();
        }

        public async Task<ApplicationDto> UpdateApplicationStatusAsync(int applicationId, UpdateApplicationStatusDto dto, int employerId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                throw new Exception("Application not found.");

            if (application.Job.CompanyId != employerId)
                throw new UnauthorizedAccessException();

            application.Status = dto.Status;
            await _context.SaveChangesAsync();

            var statusText = dto.Status == "Accepted" ? "قبول" : "رفض";
            var jobTitle = application.Job.JobTitle;
            var company = application.Job.Company;
            var companyName = company.CompanyName ?? (company.FirstName + " " + company.LastName);
            var companyEmail = company.Email;
            var companyPhone = company.Phone ?? "غير متوفر";

            var notificationContent = $"تم {statusText} طلبك للوظيفة '{jobTitle}'.";
            if (dto.Status == "Accepted")
            {
                notificationContent += $"\n\nيمكنك التواصل مع الشركة ({companyName}) عبر:\nالبريد الإلكتروني: {companyEmail}\nرقم الهاتف: {companyPhone}";
            }

            await _notificationService.SendNotificationAsync(application.UserId, "تحديث حالة التقديم", notificationContent);

            return await GetApplicationAsync(applicationId);
        }

        private async Task<ApplicationDto> GetApplicationAsync(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
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
                    Cvpath = application.User.Cvpath,
                    Location = application.User.Location,                         // ✅ إضافة
                    YearsOfExperience = application.User.YearsOfExperience,       // ✅ إضافة
                    Bio = application.User.Bio,                                   // ✅ إضافة
                    SkillsList = !string.IsNullOrEmpty(application.User.SkillsList)
                        ? JsonSerializer.Deserialize<List<string>>(application.User.SkillsList)
                        : new List<string>()
                },
                Job = new JobDto
                {
                    JobId = application.Job.JobId,
                    JobTitle = application.Job.JobTitle,
                    JobCategory = application.Job.JobCategory,
                    Location = application.Job.Location,
                    JobType = application.Job.JobType,
                    Company = new UserProfileDto
                    {
                        UserId = application.Job.Company.UserId,
                        Email = application.Job.Company.Email,
                        CompanyName = application.Job.Company.CompanyName,
                        Phone = application.Job.Company.Phone,
                        LogoPath = application.Job.Company.LogoPath
                    }
                }
            };
        }
    }
}
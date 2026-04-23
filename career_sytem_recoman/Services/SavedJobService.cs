using career_sytem_recoman.Models.DTOs.Saved;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class SavedJobService : ISavedJobService
    {
        private readonly JobPlatformContext _context;

        public SavedJobService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task SaveJobAsync(int userId, int jobId)
        {
            // التحقق من وجود الوظيفة
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                throw new Exception("Job not found.");

            // منع التكرار
            var exists = await _context.SavedJobs
                .AnyAsync(sj => sj.UserId == userId && sj.JobId == jobId);
            if (exists)
                throw new Exception("Job already saved.");

            var savedJob = new SavedJob
            {
                UserId = userId,
                JobId = jobId,
                SavedAt = DateTime.UtcNow
            };
            _context.SavedJobs.Add(savedJob);
            await _context.SaveChangesAsync();
        }

        public async Task UnsaveJobAsync(int userId, int jobId)
        {
            var savedJob = await _context.SavedJobs
                .FirstOrDefaultAsync(sj => sj.UserId == userId && sj.JobId == jobId);
            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<SavedJobDto>> GetSavedJobsAsync(int userId)
        {
            var savedJobs = await _context.SavedJobs
                .Where(sj => sj.UserId == userId)
                .Include(sj => sj.Job)
                    .ThenInclude(j => j.Company)
                .OrderByDescending(sj => sj.SavedAt)
                .Select(sj => new SavedJobDto
                {
                    Id = sj.Id,
                    JobId = sj.JobId,
                    JobTitle = sj.Job.JobTitle,
                    CompanyName = sj.Job.Company.CompanyName ?? (sj.Job.Company.FirstName + " " + sj.Job.Company.LastName),
                    SavedAt = sj.SavedAt
                })
                .ToListAsync();

            return savedJobs;
        }

        public async Task<bool> IsJobSavedAsync(int userId, int jobId)
        {
            return await _context.SavedJobs
                .AnyAsync(sj => sj.UserId == userId && sj.JobId == jobId);
        }
    }
}
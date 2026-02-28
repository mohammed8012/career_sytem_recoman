using career_sytem_recoman.Models.DTOs.Job;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services;

public class JobService(JobPlatformContext context) : IJobService
{
    public async Task<List<JobDto>> GetJobsAsync(JobFilterDto filter)
    {
        var query = context.Jobs.AsQueryable();

        if (!string.IsNullOrEmpty(filter.JobCategory))
            query = query.Where(j => j.JobCategory == filter.JobCategory);
        if (!string.IsNullOrEmpty(filter.JobType))
            query = query.Where(j => j.JobType == filter.JobType);
        if (!string.IsNullOrEmpty(filter.Location))
            query = query.Where(j => j.Location != null && j.Location.Contains(filter.Location));
        if (filter.MinExperience.HasValue)
            query = query.Where(j => j.MinExperience >= filter.MinExperience);
        if (filter.CompanyId.HasValue)
            query = query.Where(j => j.CompanyId == filter.CompanyId);

        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip(((filter.Page ?? 1) - 1) * (filter.PageSize ?? 10))
            .Take(filter.PageSize ?? 10)
            .ToListAsync();

        return [.. jobs.Select(j => new JobDto
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
        })];
    }

    public async Task<JobDto> GetJobAsync(int jobId)
    {
        var job = await context.Jobs.FindAsync(jobId);
        if (job is null)
            throw new Exception("Job not found.");

        return new JobDto
        {
            JobId = job.JobId,
            CompanyId = job.CompanyId,
            JobTitle = job.JobTitle,
            JobCategory = job.JobCategory,
            Description = job.Description,
            Requirements = job.Requirements,
            Location = job.Location,
            JobType = job.JobType,
            MinExperience = job.MinExperience,
            CreatedAt = job.CreatedAt,
            ExpiryDate = job.ExpiryDate,
            IsActive = job.IsActive
        };
    }

    public async Task<JobDto> CreateJobAsync(CreateJobDto dto, int employerId)
    {
        var job = new Job
        {
            CompanyId = employerId,
            JobTitle = dto.JobTitle,
            JobCategory = dto.JobCategory,
            Description = dto.Description,
            Requirements = dto.Requirements,
            Location = dto.Location,
            JobType = dto.JobType,
            MinExperience = dto.MinExperience,
            ExpiryDate = dto.ExpiryDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return await GetJobAsync(job.JobId);
    }

    public async Task<JobDto> UpdateJobAsync(int jobId, UpdateJobDto dto, int employerId)
    {
        var job = await context.Jobs.FindAsync(jobId);
        if (job is null)
            throw new Exception("Job not found.");
        if (job.CompanyId != employerId)
            throw new UnauthorizedAccessException();

        if (!string.IsNullOrEmpty(dto.JobTitle))
            job.JobTitle = dto.JobTitle;
        if (!string.IsNullOrEmpty(dto.JobCategory))
            job.JobCategory = dto.JobCategory;
        if (dto.Description is not null)
            job.Description = dto.Description;
        if (dto.Requirements is not null)
            job.Requirements = dto.Requirements;
        if (dto.Location is not null)
            job.Location = dto.Location;
        if (dto.JobType is not null)
            job.JobType = dto.JobType;
        if (dto.MinExperience.HasValue)
            job.MinExperience = dto.MinExperience;
        if (dto.ExpiryDate.HasValue)
            job.ExpiryDate = dto.ExpiryDate;
        if (dto.IsActive.HasValue)
            job.IsActive = dto.IsActive.Value;

        await context.SaveChangesAsync();
        return await GetJobAsync(jobId);
    }

    public async Task DeleteJobAsync(int jobId, int employerId)
    {
        var job = await context.Jobs.FindAsync(jobId);
        if (job is null)
            return;
        if (job.CompanyId != employerId)
            throw new UnauthorizedAccessException();

        context.Jobs.Remove(job);
        await context.SaveChangesAsync();
    }

    public async Task<List<JobDto>> GetJobsByCompanyAsync(int companyId)
    {
        var jobs = await context.Jobs
            .Where(j => j.CompanyId == companyId)
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

        return [.. jobs];
    }
}
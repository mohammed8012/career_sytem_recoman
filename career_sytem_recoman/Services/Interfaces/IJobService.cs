using career_sytem_recoman.Models.DTOs.Job;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IJobService
    {
        Task<List<JobDto>> GetJobsAsync(JobFilterDto filter);
        Task<JobDto> GetJobAsync(int jobId);
        Task<JobDto> CreateJobAsync(CreateJobDto dto, int employerId);
        Task<JobDto> UpdateJobAsync(int jobId, UpdateJobDto dto, int employerId);
        Task DeleteJobAsync(int jobId, int employerId);
        Task<List<JobDto>> GetJobsByCompanyAsync(int companyId);
    }
}
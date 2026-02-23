using career_sytem_recoman.Models.DTOs.Application;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<ApplicationDto> ApplyAsync(int userId, CreateApplicationDto dto);
        Task<List<ApplicationDto>> GetUserApplicationsAsync(int userId);
        Task<List<ApplicationDto>> GetJobApplicationsAsync(int jobId, int employerId);
        Task<ApplicationDto> UpdateApplicationStatusAsync(int applicationId, UpdateApplicationStatusDto dto, int employerId);
    }
}
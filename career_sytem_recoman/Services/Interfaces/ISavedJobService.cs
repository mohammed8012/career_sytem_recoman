using career_sytem_recoman.Models.DTOs.Saved;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface ISavedJobService
    {
        Task SaveJobAsync(int userId, int jobId);
        Task UnsaveJobAsync(int userId, int jobId);
        Task<List<SavedJobDto>> GetSavedJobsAsync(int userId);
        Task<bool> IsJobSavedAsync(int userId, int jobId);
    }
}
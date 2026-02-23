using career_sytem_recoman.Models.DTOs.Job;

using System.IO;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IEmployerService
    {
        Task<List<JobDto>> GetJobsByEmployerAsync(int employerId);
        Task<List<ApplicantDto>> GetApplicantsForJobAsync(int jobId, int employerId);
        Task<(Stream Stream, string ContentType, string FileName)> GetApplicantCvAsync(int applicantId, int employerId);
    }
}
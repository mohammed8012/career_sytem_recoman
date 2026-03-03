using career_sytem_recoman.Models.DTOs.CV;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IAiCvService
    {
        Task<CvAnalysisResultDto> GetFullAnalysisAsync(Stream fileStream, string fileName);
    }
}
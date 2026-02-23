using career_sytem_recoman.Models.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<string> UploadCvAsync(int userId, IFormFile file);
        Task<(Stream Stream, string ContentType, string FileName)> GetCvFileAsync(int userId);
    }
}
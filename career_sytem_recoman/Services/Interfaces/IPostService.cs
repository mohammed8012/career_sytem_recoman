using career_sytem_recoman.Models.DTOs.User;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<UserPostDto>> GetFeedAsync(int userId);
        Task<UserPostDto> UpdatePostAsync(int postId, CreatePostDto dto, int currentUserId);
        Task DeletePostAsync(int postId, int currentUserId);
    }
}
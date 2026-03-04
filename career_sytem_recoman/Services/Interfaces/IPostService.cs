using career_sytem_recoman.Models.DTOs.Posts;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<PostDto>> GetFeedAsync();
        Task<PostDto?> GetPostByIdAsync(int postId); // <-- تغيير هنا
        Task<PostDto> CreatePostAsync(int companyId, CreatePostDto dto);
        Task<PostDto> UpdatePostAsync(int postId, int companyId, CreatePostDto dto);
        Task DeletePostAsync(int postId, int companyId);
    }
}
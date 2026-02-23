using career_sytem_recoman.Models.DTOs.User;
using career_sytem_recoman.Services.Interfaces;

namespace career_sytem_recoman.Services
{
    public class PostService : IPostService
    {
        public Task<List<UserPostDto>> GetFeedAsync(int userId)
        {
            throw new NotImplementedException("Posts not implemented.");
        }

        public Task<UserPostDto> UpdatePostAsync(int postId, CreatePostDto dto, int currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task DeletePostAsync(int postId, int currentUserId)
        {
            throw new NotImplementedException();
        }
    }
}
using career_sytem_recoman.Models.DTOs.Posts;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class PostService : IPostService
    {
        private readonly JobPlatformContext _context;

        public PostService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<PostDto>> GetFeedAsync()
        {
            var posts = await _context.Posts
                .Include(p => p.Company)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto
                {
                    PostId = p.PostId,
                    CompanyId = p.CompanyId,
                    CompanyName = p.Company.CompanyName ?? p.Company.FirstName + " " + p.Company.LastName,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return posts;
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId) // <-- تغيير هنا
        {
            var post = await _context.Posts
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null)
                return null; // <-- بدلاً من throw

            return new PostDto
            {
                PostId = post.PostId,
                CompanyId = post.CompanyId,
                CompanyName = post.Company.CompanyName ?? post.Company.FirstName + " " + post.Company.LastName,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt
            };
        }

        public async Task<PostDto> CreatePostAsync(int companyId, CreatePostDto dto)
        {
            var company = await _context.Users.FindAsync(companyId);
            if (company == null || company.UserType != "Company")
                throw new Exception("Only companies can create posts.");

            var post = new Post
            {
                CompanyId = companyId,
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // بعد الإضافة، نحتاج إلى جلب المنشور مع بيانات الشركة
            return await GetPostByIdAsync(post.PostId) ?? throw new Exception("Failed to retrieve created post.");
        }

        public async Task<PostDto> UpdatePostAsync(int postId, int companyId, CreatePostDto dto)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");
            if (post.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not authorized to update this post.");

            post.Title = dto.Title;
            post.Content = dto.Content;
            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(postId) ?? throw new Exception("Failed to retrieve updated post.");
        }

        public async Task DeletePostAsync(int postId, int companyId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return;
            if (post.CompanyId != companyId)
                throw new UnauthorizedAccessException("You are not authorized to delete this post.");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}
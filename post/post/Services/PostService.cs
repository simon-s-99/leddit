using System.Threading.Tasks;
using post.Models;
using post.Models.DTOs;
using post.Data;

namespace post.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreatePostAsync(CreatePostDTO dto)
        {
            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = dto.UserId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post.Id;
        }

        public async Task UpdatePostAsync(int id, UpdatePostDTO dto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                throw new System.Exception("Post not found.");
            }

            post.Title = dto.Title;
            post.Content = dto.Content;

            await _context.SaveChangesAsync();
        }
    }
}




using System.Threading.Tasks;
using post.Models;
using post.Models.DTOs;
using post.Data;

namespace post.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;
        private readonly MessageService _messageService;

        public PostService(ApplicationDbContext context, MessageService messageService)
        {
            _context = context;
            _messageService = messageService;
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
            _messageService.NotifyPostChanged("add-post", post);
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
            _messageService.NotifyPostChanged("update-post", post);
        }
    }
}




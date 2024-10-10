using System.Threading.Tasks;
using LedditModels;
using post.Data;
using post.DTOs;

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

        public async Task<Guid> CreatePostAsync(CreatePostDTO dto)
        {
            if (dto.UserId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required.");
            }
            
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

        public async Task UpdatePostAsync(Guid id, UpdatePostDTO dto)
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

        public async Task DeletePostAsync(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                throw new System.Exception("Post not found.");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            _messageService.NotifyPostChanged("delete-post", post);
            _messageService.NotifyPostChanged("delete-post-comment", post);

        }

        public async Task<Post> GetPostAsync(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                throw new System.Exception("Post not found.");
            }
            return post;
        }
    }
}




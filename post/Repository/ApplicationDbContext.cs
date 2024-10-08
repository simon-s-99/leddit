using Microsoft.EntityFrameworkCore;
using LedditModels;

namespace post.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }

    }
}

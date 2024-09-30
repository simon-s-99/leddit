using Microsoft.EntityFrameworkCore;
using LedditModels;

namespace Comments.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Comment> Comments { get; set; }
    }
}
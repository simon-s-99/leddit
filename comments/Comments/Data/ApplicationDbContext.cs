using Microsoft.EntityFrameworkCore;
using Comments.Models;

namespace Comments.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Comment> Comments { get; set; }
    }
}
using Microsoft.EntityFrameworkCore;
using LedditModels;

namespace Logs.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Log> Logs { get; set; }
    }
}
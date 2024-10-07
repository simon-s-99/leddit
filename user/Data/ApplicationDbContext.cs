using LedditModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace userIdentityAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Map custom ApplicationUser properties explicitly
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.UserName).HasColumnType("text");
                entity.Property(e => e.NormalizedUserName).HasColumnType("text");
                entity.Property(e => e.Email).HasColumnType("text");
                entity.Property(e => e.NormalizedEmail).HasColumnType("text");
                entity.Property(e => e.PasswordHash).HasColumnType("text");
                entity.Property(e => e.SecurityStamp).HasColumnType("text");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("text");

                // Custom properties
                entity.Property(e => e.DisplayName).HasColumnType("text");
                entity.Property(e => e.Bio).HasColumnType("text");
                entity.Property(e => e.ProfilePictureUrl).HasColumnType("text");

                entity.Property(e => e.Karma).HasDefaultValue(0); // Default value for Karma
                entity.Property(e => e.DateOfBirth).HasColumnType("timestamp"); // Mapping DateTime to PostgreSQL-compatible type
            });

            // IdentityRole properties mapping to PostgreSQL types
            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.Name).HasColumnType("text");
                entity.Property(e => e.NormalizedName).HasColumnType("text");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("text");
            });
        }
    }
}

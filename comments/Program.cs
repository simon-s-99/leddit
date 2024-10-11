
using System.Net;
using Comments.Repository;
using Comments.Services;
using Microsoft.EntityFrameworkCore;

namespace Comments
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            var connectionString = builder.Configuration.GetConnectionString("kubernetes") ?? throw new InvalidOperationException("Connection string not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddScoped<CommentsService>();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddHostedService<MessageService>();
            builder.Services.AddSingleton(s => s.GetServices<IHostedService>().OfType<MessageService>().First());

            builder.Services.AddHttpClient();
            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            // ====== Migrate database on startup
            // this works since we only run one instance of posts at a time
            // this is generally not recommended for production deployments due to the instability it could cause
            // Other solutions seem to require Helm which is outside of the scope of this assignment. 
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}

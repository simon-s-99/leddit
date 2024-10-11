
using Logs.Repository;
using Logs.Services;
using Microsoft.EntityFrameworkCore;

namespace Logs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("kubernetes") ?? throw new InvalidOperationException("Connection string not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddScoped<LogsService>();
            builder.Services.AddHostedService<MessageService>();
            builder.Services.AddSingleton(s => s.GetServices<IHostedService>().OfType<MessageService>().First());

            var app = builder.Build();

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

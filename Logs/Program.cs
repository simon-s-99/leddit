
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
            app.Run();
        }
    }
}

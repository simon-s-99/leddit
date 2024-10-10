
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

            builder.Services.AddControllers();

            var connectionString = builder.Configuration.GetConnectionString("kubernetes") ?? throw new InvalidOperationException("Connection string not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddScoped<LogsService>();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddHostedService<MessageService>();
            builder.Services.AddSingleton(s => s.GetServices<IHostedService>().OfType<MessageService>().First());

            builder.Services.AddHttpClient();
            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

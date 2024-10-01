using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using userIdentityAPI.Data;
using userIdentityAPI.Services;
using userIdentityAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace userIdentityAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load User Secrets for development environment
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Identity and authentication services
            //builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
            //    options.Password.RequireDigit = true;           // Require at least one digit
            //    options.Password.RequiredLength = 6;            // Minimum password length
            //    options.Password.RequireNonAlphanumeric = false; // Do not require special characters
            //    options.Password.RequireUppercase = true;       // Require at least one uppercase letter
            //    options.Password.RequireLowercase = true;       // Require at least one lowercase letter
            //}).AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation for now
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();  // Add this to generate tokens for password reset, email confirmation, etc.



            // Add JWT Authentication
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Add Google Authentication
            builder.Services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });

            // Add Authorization
            builder.Services.AddAuthorization();

            // Register MVC Controllers
            builder.Services.AddControllers();

            builder.Services.AddRazorPages();

            // Add RabbitMQProducer as a hosted service
            builder.Services.AddHostedService<RabbitMQProducer>();
            builder.Services.AddSingleton(s => s.GetServices<IHostedService>().OfType<RabbitMQProducer>().First());

            // Add Swagger and API Explorer
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();  // Moved service registration before this line

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Enable Swagger in development mode
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Map minimal API with OpenAPI
            //app.MapGet(
            //    "/user/{id}",
            //    (int id) =>
            //    {
            //        var user = new
            //        {
            //            Id = 1,
            //            Name = "John Doe",
            //            Email = "john.doe@example.com"
            //        };
            //        return Results.Ok(user);
            //    }
            //)
            //.WithName("GetUser")
            //.WithOpenApi();

            app.MapGet(
        "/user",
        () =>
        {
            return "Hello from user!";
        }
    )
    .WithName("User")
    .WithOpenApi();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}

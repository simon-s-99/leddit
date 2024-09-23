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

            //HttpClient client = new HttpClient { BaseAddress = new Uri("http://10.110.47.63:8080/") }; // uri needs to change?

            //var webRequest = new HttpRequestMessage(HttpMethod.Get, "api/movies?search=test");

            //var response = client.Send(webRequest);
            //Console.WriteLine("Sent");
            //Console.WriteLine(response);

            //// Läs in kropp i form av JSON och omvandla till objekt
            //using var reader = new StreamReader(response.Content.ReadAsStream());
            //var json = reader.ReadToEnd();
            //Console.WriteLine(json);


            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            // Add JWT Authentication. Using UTF8 in order to make the key the required amount of bytes for postman (256)
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


            builder.Services.AddControllersWithViews();

            // Add Google Authentication
            builder.Services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });

            // Register RabbitMQProducer as a hosted service
            builder.Services.AddHostedService<RabbitMQProducer>();
            builder.Services.AddSingleton(s => s.GetServices<IHostedService>().OfType<RabbitMQProducer>().First());

            var app = builder.Build();          

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
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

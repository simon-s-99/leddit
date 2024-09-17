
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Search
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // temporary connection solution, replace when deploying prod docker elastic container 
            // Prod solution would need more configuration, more info here:
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html 
            string elasticConnString = "http://127.0.0.1:9200";
            string elasticUsername = "elastic";
            string elasticPassword = "dev";
            var elasticSettings = new ElasticsearchClientSettings(new Uri(elasticConnString))
                .Authentication(new BasicAuthentication(elasticUsername, elasticPassword));
            builder.Services.AddSingleton<ElasticsearchClient>(new ElasticsearchClient(elasticSettings));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer(); // swagger related? Remove?
            builder.Services.AddSwaggerGen(); // swagger related? Remove?

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            // {
            //     app.UseSwagger();
            //     app.UseSwaggerUI();
            // }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

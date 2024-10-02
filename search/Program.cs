using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Search
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // add elastisearchclient connection settings as a singleton
            // this is recommended in elasticsearchs documentation 
            builder.Services.AddSingleton<IElasticsearchClientSettings, ElasticsearchClientSettings>(sp =>
            {
                string elasticConnString = "http://127.0.0.1:9200";
                string elasticUsername = "elastic";
                string elasticPassword = "dev"; // change this in an actual production environment 
                var elasticSettings = new ElasticsearchClientSettings(new Uri(elasticConnString))
                    .Authentication(new BasicAuthentication(elasticUsername, elasticPassword));
                return elasticSettings;
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

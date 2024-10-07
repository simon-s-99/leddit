using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using LedditModels;

namespace Search
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // add elastisearchclient connection settings as a singleton
            // this is recommended in elasticsearchs documentation & makes sense
            // since elasticsearch indexes based on connectionsettings 
            builder.Services.AddSingleton<IElasticsearchClientSettings, ElasticsearchClientSettings>(sp =>
            {
                // connectionString dns name works over a docker network 
                string elasticConnString =  "http://elasticsearch:9200";
                string elasticUsername = "elastic";
                string elasticPassword = "dev"; // change this in an actual production environment 
                var elasticSettings = new ElasticsearchClientSettings(new Uri(elasticConnString))
                    .DefaultMappingFor<ApplicationUser>(m => m.IndexName("users"))
                    .DefaultMappingFor<Post>(m => m.IndexName("posts"))
                    .DefaultMappingFor<Comment>(m => m.IndexName("comments"))
                    .Authentication(new BasicAuthentication(elasticUsername, elasticPassword))
                    .EnableDebugMode(); // allows .DebugInformation on requests to ElasticSearch
                return elasticSettings;
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            app.Urls.Add("http://*:9201");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

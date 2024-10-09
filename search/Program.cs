using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using LedditModels;
using Search.Services;

namespace Search
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // add elastisearchclient _connection settings as a singleton
            // this is recommended in elasticsearchs documentation & makes sense
            // since elasticsearch indexes based on connectionsettings 
            builder.Services.AddSingleton<IElasticsearchClientSettings, ElasticsearchClientSettings>(sp =>
            {
                // connectionString dns name works over k8s
                // (elastic does not have a built in way of retrieving a connectionsring
                // from appsettings.json, this can be done manually but is cumbersome)
                string elasticConnString = "https://localhost:9200"; //"https://elasticsearch-svc.default.svc.cluster.local:9200";
                string elasticUsername = "elastic";
                string elasticPassword = "dev"; // change this in an actual production environment 
                var elasticSettings = new ElasticsearchClientSettings(new Uri(elasticConnString))
                    .Authentication(new BasicAuthentication(elasticUsername, elasticPassword))
                    //.EnableDebugMode() // allows .DebugInformation on requests to ElasticSearch
                    .DefaultMappingFor<ApplicationUser>(m => m.IndexName("users"))
                    .DefaultMappingFor<Post>(m => m.IndexName("posts"))
                    .DefaultMappingFor<Comment>(m => m.IndexName("comments"));
                return elasticSettings;
            });

            builder.Services.AddScoped<SearchService>();

            builder.Services.AddControllers();

            var app = builder.Build();

            // these only work for local testing or docker testing, does not work with k8s
            app.Urls.Add("http://*:9201"); // allows http connections over port 9201
            //app.Urls.Add("https://*:9201"); // allows https connections over port 9201

            // this has no port to redirect to but should be used in actual deployment
            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

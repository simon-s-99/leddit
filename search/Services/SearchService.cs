using Elastic.Clients.Elasticsearch;
using Newtonsoft.Json;
using LedditModels;

namespace Search.Services
{
    public static class SearchService
    {

        // create multiple overloads that perform the search with 
        // the given datamodel & query 

        // search based on the searchTerm
        // do either free-text search if possible or do 
        // search for one datamodel, then the next, then the next
        // & put this in one list sorted in descending order for "searchWeight" /score

        public static async Task<List<string>> SearchAsync(
            IElasticsearchClientSettings elasticsearchClientSettings,
            string searchTerm)
        {
            // ElasticClient is thread-safe and does not implement IDispose
            var client = new ElasticsearchClient(elasticsearchClientSettings);

            // stores results in list of JSON strings
            List<string> searchResults = new();

            var commentSearchResponse = await client.SearchAsync<Comment>(s => s
                .Index("comments")
                .From(0)
                .Size(100)
                .Query(q => q
                    .MatchPhrase(m => m
                        .Field(f => f.Body)
                        .Query(searchTerm)
                    )
                )
            );

            //Console.WriteLine(commentSearchResponse.DebugInformation); // for debugging

            if (commentSearchResponse.IsValidResponse)
            {
                foreach (var response in commentSearchResponse.Documents)
                {
                    string jsonResponse = JsonConvert.SerializeObject(response);
                    searchResults.Add(jsonResponse);
                }

                return searchResults;
            }

            searchResults.Add("No results found OR Invalid response from DB");
            return searchResults;
        }
    }
}

using Elastic.Clients.Elasticsearch;
using LedditModels;

namespace Search.Services
{
    public class SearchService
    {
        private readonly IElasticsearchClientSettings _elasticsearchClientSettings;

        public SearchService(IElasticsearchClientSettings elasticsearchClientSettings)
        {
            _elasticsearchClientSettings = elasticsearchClientSettings;
        }

        public async Task<List<List<string>>> SearchAsync(string searchTerm)
        {
            // ElasticClient is thread-safe and does not implement IDispose
            var client = new ElasticsearchClient(_elasticsearchClientSettings);

            var postSearchResponse = await client.SearchAsync<Post>(s => s
                .From(0) // provides 0 -->
                .Size(5) // --> to 5 hits
                .Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.Content)
                        .Query(searchTerm)
                        .Slop(2) // how many positions a word can be moved for a match
                    )
                )
            );

            var commentSearchResponse = await client.SearchAsync<Comment>(s => s
                .From(0) // provides 0 -->
                .Size(5) // --> to 5 hits
                .Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.Body)
                        .Query(searchTerm)
                        .Slop(2) // how many positions a word can be moved for a match
                    )
                )
            );

            var userSearchResponse = await client.SearchAsync<ApplicationUser>(s => s
                .From(0) // provides 0 -->
                .Size(5) // --> to 5 hits
                .Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.Email)
                        .Query(searchTerm)
                        .Slop(2) // how many positions a word can be moved for a match
                    )
                )
            );

            //Console.WriteLine("SearchResponses debug info :");
            //Console.WriteLine("Comments = " + commentSearchResponse.DebugInformation); // for debugging
            //Console.WriteLine("Posts = " + postSearchResponse.DebugInformation); // for debugging
            //Console.WriteLine("Users = " + userSearchResponse.DebugInformation); // for debugging

            // Add responses formatted to json in returnvalue
            List<string> postSearchResults = new(["Type: Posts", .. ResponseToJson<Post>(postSearchResponse)]);
            List<string> commentSearchResults = new(["Type: Comments", .. ResponseToJson<Comment>(commentSearchResponse)]);
            List<string> userSearchResults = new(["Type: Users", .. ResponseToJson<ApplicationUser>(userSearchResponse)]);

            // stores results in 2 dimensional list of JSON strings
            List<List<string>> searchResults = new([[.. postSearchResults], [.. commentSearchResults], [.. userSearchResults]]);

            return searchResults;
        }

        // add/index a document in elasticsearch db 
        public async void IndexDocument<T>(T document)
        {
            // ElasticClient is thread-safe and does not implement IDispose
            var client = new ElasticsearchClient(_elasticsearchClientSettings);

            var response = await client.IndexAsync<T>(document);

            if (!response.IsValidResponse)
            {
                throw new ArgumentException("Invalid Document passed to indexing or DB is unavailable.");
            }
        }

        /// <summary>
        /// Turns ElasticSearch SearchResponses into formatted JSON-strings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchResponse"></param>
        /// <returns>A list of responses formatted as JSON-strings.</returns>
        internal List<string> ResponseToJson<T>(SearchResponse<T> searchResponse)
        {
            // Stores results 
            List<string> results = new();

            if (searchResponse.IsValidResponse)
            {
                if (searchResponse.Hits.Count <= 0)
                {
                    results.Add($"No hits when searching for {typeof(T).ToString()}");
                }
                else
                {
                    foreach (var response in searchResponse.Documents)
                    {
                        string jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                        results.Add(jsonResponse);
                    }
                }
            }
            else
            {
                results.Add($"Invalid response when searching for {typeof(T).ToString()}");
            }

            return results;
        }
    }
}

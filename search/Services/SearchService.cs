using Elastic.Clients.Elasticsearch;
using Newtonsoft.Json;
using LedditModels;

namespace Search.Services
{
    public class SearchService
    {
        private ElasticsearchClient _client;

        public SearchService(IElasticsearchClientSettings elasticsearchClientSettings)
        {
            // ElasticClient is thread-safe and does not implement IDispose
            _client = new ElasticsearchClient(elasticsearchClientSettings);
        }

        public async Task<List<List<string>>> SearchAsync(string searchTerm)
        {
            // stores results in list of JSON strings
            List<List<string>> searchResults = new();

            var postSearchResponse = await _client.SearchAsync<Post>(s => s
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

            var commentSearchResponse = await _client.SearchAsync<Comment>(s => s
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

            var userSearchResponse = await _client.SearchAsync<ApplicationUser>(s => s
                .From(0) // provides 0 -->
                .Size(5) // --> to 5 hits
                .Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.DisplayName)
                        .Query(searchTerm)
                        .Slop(2) // how many positions a word can be moved for a match
                    )
                )
            );

            //Console.WriteLine(commentSearchResponse.DebugInformation); // for debugging

            // Add responses formatted to json in returnvalue
            List<string> postSearchResults = new();
            List<string> commentSearchResults = new();
            List<string> userSearchResults = new();

            postSearchResults.Add("Type: Posts");
            commentSearchResults.Add("Type: Comments");
            userSearchResults.Add("Type: Users");

            postSearchResults.AddRange(ResponseToJson<Post>(postSearchResponse));
            commentSearchResults.AddRange(ResponseToJson<Comment>(commentSearchResponse));
            userSearchResults.AddRange(ResponseToJson<ApplicationUser>(userSearchResponse));

            searchResults.Add(postSearchResults);
            searchResults.Add(commentSearchResults);
            searchResults.Add(userSearchResults);

            return searchResults;
        }

        // add/index a document in elasticsearch db 
        public async void IndexDocument<T>(T document)
        {
            var response = await _client.IndexAsync<T>(document);
            if (!response.IsValidResponse) { throw new ApplicationException("Invalid response from ES"); }
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
                        string jsonResponse = JsonConvert.SerializeObject(response);
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

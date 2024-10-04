using Elastic.Clients.Elasticsearch;
using LedditModels;
using Search.Models;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

namespace Search.Services
{
    public static class SearchService
    {

        // create multiple overloads that perform the search with 
        // the given datamodel & query 

        // search based on the searchTerm
        // do either free-text search if possible or do 
        // search for one datamodel, then the next, then the next
        // & put this in one list sorted in descending order for "searchWeight" 

        public static async Task<List<SearchResult>> SearchAsync(
            IElasticsearchClientSettings elasticsearchClientSettings,
            string searchTerm)
        {
            // ElasticClient is thread-safe and does not implement IDispose
            var client = new ElasticsearchClient(elasticsearchClientSettings);

            var commentSearchResponse = await client.SearchAsync<Comment>(s => s
                .Index("comment-index") // index to query
                .From(0)
                .Size(5)
                .Query(q => q
                    .Term(t => t.Field("Body").Value("hi")) // searchTerm
                )
            );

            List<SearchResult> results = new();

            if (commentSearchResponse.IsValidResponse)
            {
                var comment = commentSearchResponse.Documents.FirstOrDefault();

                SearchResult result = new();
                //result.Test = comment.ToString();

                result.Test = "success";

                results.Add(result);

                return results;
            }

            SearchResult fail = new();
            fail.Test = "fail";

            results.Add(fail);

            return results;
        }
    }
}

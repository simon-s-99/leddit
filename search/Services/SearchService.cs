﻿using Elastic.Clients.Elasticsearch;
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
                .From(0) // provides 0 -->
                .Size(100) // --> to 100 hits
                .Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.Body)
                        .Query(searchTerm)
                        .Slop(2) // how many positions a word can be moved for a match
                    )
                )
            );

            //Console.WriteLine(commentSearchResponse.DebugInformation); // for debugging

            if (commentSearchResponse.IsValidResponse)

        /// <summary>
        /// Turns ElasticSearch SearchResponses into formatted JSON-strings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchResponse"></param>
        /// <returns>A list of responses formatted as JSON-strings.</returns>
        internal static List<string> ResponseToJson<T>(SearchResponse<T> searchResponse)
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

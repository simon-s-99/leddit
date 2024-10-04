﻿using Elastic.Clients.Elasticsearch;
using LedditModels;
using Search.Models;

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

        public static IEnumerable<SearchResult> Search(
            ElasticsearchClientSettings elasticsearchClientSettings,
            string searchTerm)
        {
            // ElasticClient is thread-safe and does not implement IDispose
            var client = new ElasticsearchClient(elasticsearchClientSettings);

            var commentSearchResponse = client.SearchAsync<Comment>(s => s
                .Index("") // index to query
                .From(0)
                .Size(2)
                .Query(q => q
                    .Term() // column to query & actual query/term 
                )
            );
        }
    }
}
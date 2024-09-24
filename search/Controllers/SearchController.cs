using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Search.Models;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute; // avoids ambigous reference error

namespace Search.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchClientSettings _elasticsearchClientSettings;

        public SearchController(IElasticsearchClientSettings elasticsearchClientSettings)
        {
            _elasticsearchClientSettings = elasticsearchClientSettings;
        }

        [HttpGet(Name = "search")]
        public IEnumerable<SearchResult> Get()
        {
            var client = new ElasticsearchClient(_elasticsearchClientSettings);

            // TODO - implement actual searching here, connection seems to be working 

            // return Enumerable.ToArray() or something similar 
            SearchResult[] searchResults = []; // <-- remove this later, temporary code 
            return searchResults; // <-- remove this later, temporary code
        }
    }
}

using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Search.Models;
using Search.Services;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute; // avoids ambigous reference error

namespace Search.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ElasticsearchClientSettings _elasticsearchClientSettings;

        public SearchController(ElasticsearchClientSettings elasticsearchClientSettings)
        {
            _elasticsearchClientSettings = elasticsearchClientSettings;
        }

        [HttpGet(Name = "search")]
        public IEnumerable<SearchResult> Get([FromQuery] string searchTerm)
        {
            // runs search logic & returns searchresults 
            IEnumerable<SearchResult> searchResults = SearchService.Search(_elasticsearchClientSettings, searchTerm);

            return searchResults;
        }
    }
}

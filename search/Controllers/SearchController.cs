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
        private readonly IElasticsearchClientSettings _elasticsearchClientSettings;

        public SearchController(IElasticsearchClientSettings elasticsearchClientSettings)
        {
            _elasticsearchClientSettings = elasticsearchClientSettings;
        }

        [HttpGet(Name = "search")]
        public List<SearchResult> Get([FromQuery] string q)
        {
            string searchTerm = q;
            // runs search logic & returns searchresults 
            List<SearchResult> searchResults =
                SearchService.SearchAsync(_elasticsearchClientSettings, searchTerm).Result;

            return searchResults;
        }
    }
}

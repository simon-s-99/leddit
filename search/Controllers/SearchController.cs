using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Search.Services;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute; // avoids ambigous reference error

namespace Search.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchClientSettings _elasticsearchClientSettings;

        public SearchController(IElasticsearchClientSettings elasticsearchClientSettings)
        {
            _elasticsearchClientSettings = elasticsearchClientSettings;
        }

        [HttpGet(Name = "search")]
        public List<List<string>> Get([FromQuery(Name = "q")] string searchTerm)
        {
            // runs search logic & returns searchresults 
            List<List<string>> searchResults = 
                SearchService.SearchAsync(_elasticsearchClientSettings, searchTerm).Result;

            return searchResults;
        }
    }
}

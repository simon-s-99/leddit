using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
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
        public List<List<string>> Get([FromQuery(Name = "q")] string searchTerm)
        {
            // run search logic & return 2 dimensional list (easier response readability)
            List<List<string>> searchResults =
                new SearchService(_elasticsearchClientSettings).SearchAsync(searchTerm).Result;

            return searchResults;
        }
    }
}

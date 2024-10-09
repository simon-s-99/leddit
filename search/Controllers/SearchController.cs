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
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet(Name = "search")]
        public List<List<string>> Get([FromQuery(Name = "q")] string searchTerm)
        {
            // runs search logic & returns searchresults 
            List<List<string>> searchResults = _searchService.SearchAsync(searchTerm).Result;

            return searchResults;
        }
    }
}

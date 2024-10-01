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
        public IEnumerable<SearchResult> Get([FromQuery] string searchTerm)
        {
            // would love to dispose this but it's thread safe & does not 
            // implement IDispose soooo???? no?
            var client = new ElasticsearchClient(_elasticsearchClientSettings);

            // TODO - implement actual searching here, connection seems to be working 

            // search based on the searchTerm
            // do either free-text search if possible or do 
            // search for one datamodel, then the next, then the next
            // & put this in one list sorted in descending order for "searchWeight" 

            var query = Request.QueryString;
            var result = new SearchResult();
            result.Test = query.ToString();

            List<SearchResult> searchResults = new();

            searchResults.Add(result);

            return searchResults;
        }
    }
}

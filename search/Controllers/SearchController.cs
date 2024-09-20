using Microsoft.AspNetCore.Mvc;
using Search.Models;

namespace Search.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        [HttpGet(Name = "Search")]
        public IEnumerable<SearchResult> Get()
        {
            // return Enumerable.ToArray() or something similar 
            SearchResult[] searchResults = []; // <-- remove this later, temporary code 
            return searchResults; // <-- remove this later, temporary code
        }

        //
        // Implement search/querying the elasticsearch db
        // info here: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/getting-started-net.html#_operations 
        //
    }
}
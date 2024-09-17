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
    }
}

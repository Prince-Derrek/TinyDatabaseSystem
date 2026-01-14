using Microsoft.AspNetCore.Mvc;
using TinyDB.Core.Storage;
using TinyDB.Core.Parsing;
using TinyDB.Core.Execution;

namespace TinyDB.Web.Controllers
{
    // Simple DTO for the request
    public class QueryRequest
    {
        public string Sql { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly Engine _engine;

        // Inject the Singleton Engine
        public QueryController(Engine engine)
        {
            _engine = engine;
        }

        [HttpPost]
        public IActionResult Execute([FromBody] QueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sql))
                return BadRequest("Query cannot be empty.");

            try
            {
                // 1. Tokenize
                var tokenizer = new Tokenizer(request.Sql);
                var tokens = tokenizer.Tokenize();

                // 2. Parse & Execute
                var parser = new Parser(tokens, _engine);
                var result = parser.Parse();

                // 3. Return structured JSON
                return Ok(new
                {
                    Message = result.Message,
                    Success = true,
                    Data = result.IsQuery ? result.Rows : null,
                    Columns = result.IsQuery ? result.Columns : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}
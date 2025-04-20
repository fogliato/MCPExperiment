using MCPEcommerce.Core.Interfaces;
using MCPEcommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace MCPEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IDatabaseService _databaseService;

        public QueryController(IAIService aiService, IDatabaseService databaseService)
        {
            _aiService = aiService;
            _databaseService = databaseService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessQuery([FromBody] Query query)
        {
            try
            {
                if (string.IsNullOrEmpty(query.Question))
                {
                    return BadRequest(new { error = "The question cannot be empty" });
                }

                // Generate SQL query from natural language question
                var sqlQuery = await _aiService.GenerateSQLQueryAsync(query.Question);

                // Execute SQL query
                var result = await _databaseService.ExecuteQueryAsync(sqlQuery);

                // Format result in a user-friendly way
                var formattedResult = _databaseService.FormatQueryResult(result);

                // Process the result with AI to generate a natural language response
                var answer = await _aiService.ProcessQueryAsync(
                    $"Based on the following data: {formattedResult}, answer the question: {query.Question}"
                );

                query.Answer = answer;
                query.CreatedAt = DateTime.UtcNow;
                query.ProcessedBy = "Gemini Pro";

                return Ok(query);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

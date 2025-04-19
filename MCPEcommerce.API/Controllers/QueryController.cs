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
                    return BadRequest(new { error = "A pergunta não pode ser vazia" });
                }

                // Gera a consulta SQL a partir da pergunta em linguagem natural
                var sqlQuery = await _aiService.GenerateSQLQueryAsync(query.Question);

                // Executa a consulta SQL
                var result = await _databaseService.ExecuteQueryAsync(sqlQuery);

                // Formata o resultado de forma amigável
                var formattedResult = _databaseService.FormatQueryResult(result);

                // Processa o resultado com IA para gerar uma resposta em linguagem natural
                var answer = await _aiService.ProcessQueryAsync(
                    $"Com base nos seguintes dados: {formattedResult}, responda à pergunta: {query.Question}"
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

using System.Text;
using System.Text.Json;
using MCPEcommerce.Core.Interfaces;
using MCPEcommerce.Core.Models;

namespace MCPEcommerce.Infrastructure.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IDatabaseSchemaService _schemaService;
        private const string GEMINI_API_URL =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent";

        public AIService(string apiKey, IDatabaseSchemaService schemaService)
        {
            _apiKey = apiKey;
            _schemaService = schemaService;
            _httpClient = new HttpClient();
        }

        public async Task<string> ProcessQueryAsync(string question)
        {
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $"Process the following question and generate a clear and concise response: {question}"
                            }
                        }
                    }
                }
            };

            var response = await SendRequestAsync(request);
            return response;
        }

        public async Task<string> GenerateSQLQueryAsync(string naturalLanguageQuery)
        {
            var schemaDescription = await _schemaService.GetDatabaseSchemaAsync();
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $@"Based on the following database structure:

{schemaDescription}

Generate a valid SQL query for MySQL that answers the following question: {naturalLanguageQuery}

The query should:
1. Use the exact table and column names as in the structure
2. Consider only non-deleted records (Deleted = 0) in all tables
3. Use correct MySQL syntax
4. Include all necessary JOINs to get related information
5. Use appropriate aliases for tables
6. Use the correct MySQL date format (YYYY-MM-DD)
7. Return ONLY the SQL query, with no additional text, no code markers, no explanations
8. Do not include triple quotes or code markers like ```sql

Example of expected structure:
SELECT t1.field1, t2.field2
FROM Table1 t1
INNER JOIN Table2 t2 ON t1.id = t2.id_table1
WHERE t1.Deleted = 0 AND t2.Deleted = 0"
                            }
                        }
                    }
                }
            };

            var response = await SendRequestAsync(request);
            return CleanSQLQuery(response);
        }

        private string CleanSQLQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("A query SQL não pode ser vazia");

            // Remove marcadores de código e aspas triplas
            query = query.Replace("```sql", "").Replace("```", "").Trim();

            // Remove linhas vazias no início e fim
            query = query.TrimStart('\r', '\n').TrimEnd('\r', '\n');

            // Validações básicas
            if (!query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("A query deve começar com SELECT");

            if (!query.Contains("FROM", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("A query deve conter a cláusula FROM");

            return query;
        }

        private async Task<string> SendRequestAsync(object request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Enviando requisição para: {GEMINI_API_URL}");

                var requestUrl = $"{GEMINI_API_URL}?key={_apiKey}";
                Console.WriteLine($"URL completa: {requestUrl}");
                Console.WriteLine($"Request Body: {json}");

                var response = await _httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseContent}");

                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var text = responseObj
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? throw new InvalidOperationException("Resposta vazia da API");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar requisição: {ex.Message}");
                throw;
            }
        }
    }
}

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
                                text = $"Processe a seguinte pergunta e gere uma resposta clara e concisa: {question}"
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
                                text = $@"Com base na seguinte estrutura do banco de dados:

{schemaDescription}

Gere uma consulta SQL válida para MySQL que responda à seguinte pergunta: {naturalLanguageQuery}

A consulta deve:
1. Usar os nomes exatos das tabelas e colunas conforme a estrutura
2. Considerar apenas registros não deletados (Deletado = 0) em todas as tabelas
3. Usar a sintaxe correta do MySQL
4. Incluir todos os JOINs necessários para obter informações relacionadas
5. Usar aliases apropriados para as tabelas
6. Usar o formato de data correto do MySQL (YYYY-MM-DD)
7. Retornar APENAS a consulta SQL, sem nenhum texto adicional, sem marcadores de código, sem explicações
8. Não incluir aspas triplas ou marcadores de código como ```sql

Exemplo de estrutura esperada:
SELECT t1.campo1, t2.campo2
FROM Tabela1 t1
INNER JOIN Tabela2 t2 ON t1.id = t2.id_tabela1
WHERE t1.Deletado = 0 AND t2.Deletado = 0"
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

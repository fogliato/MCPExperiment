using Microsoft.AspNetCore.Mvc;
using MCPEcommerce.Core.Interfaces;
using System.Net.Http;
using System.Text.Json;

namespace MCPEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TestController(IAIService aiService, IConfiguration configuration)
        {
            _aiService = aiService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpGet("gemini")]
        public async Task<IActionResult> TestGemini()
        {
            try
            {
                var result = await _aiService.ProcessQueryAsync("Olá, como você está?");
                return Ok(new { success = true, result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message, details = ex.ToString() });
            }
        }

        [HttpGet("list-models")]
        public async Task<IActionResult> ListModels()
        {
            try
            {
                var apiKey = _configuration["GeminiApiKey"];
                var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                
                Console.WriteLine($"Enviando requisição para: {url}");
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Content: {content}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { success = false, error = content });
                }
                
                return Ok(new { success = true, content });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message, details = ex.ToString() });
            }
        }
    }
} 
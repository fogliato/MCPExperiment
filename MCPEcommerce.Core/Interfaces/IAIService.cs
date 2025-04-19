namespace MCPEcommerce.Core.Interfaces
{
    public interface IAIService
    {
        Task<string> ProcessQueryAsync(string question);
        Task<string> GenerateSQLQueryAsync(string naturalLanguageQuery);
    }
}

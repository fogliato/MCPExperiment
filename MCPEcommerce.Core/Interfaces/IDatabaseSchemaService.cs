namespace MCPEcommerce.Core.Interfaces
{
    public interface IDatabaseSchemaService
    {
        Task<string> GetDatabaseSchemaAsync();
    }
} 
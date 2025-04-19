using System.Data;

namespace MCPEcommerce.Core.Interfaces
{
    public interface IDatabaseService
    {
        Task<DataTable> ExecuteQueryAsync(string sqlQuery);
        Task<string> GetTableSchemaAsync(string tableName);
        string FormatQueryResult(DataTable dataTable);
    }
}

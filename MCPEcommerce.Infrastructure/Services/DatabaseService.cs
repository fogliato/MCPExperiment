using System.Data;
using System.Text;
using MCPEcommerce.Core.Interfaces;
using MySql.Data.MySqlClient;

namespace MCPEcommerce.Infrastructure.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DataTable> ExecuteQueryAsync(string sqlQuery)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(sqlQuery, connection))
                    {
                        var dataTable = new DataTable();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }
        }

        public string FormatQueryResult(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return "No results found.";

            var sb = new StringBuilder();

            // For simple count or aggregation queries
            if (dataTable.Columns.Count == 1 && dataTable.Rows.Count == 1)
            {
                var value = dataTable.Rows[0][0];
                sb.AppendLine($"Result: {value}");
                return sb.ToString();
            }

            // For queries that return multiple columns
            sb.AppendLine("Results found:");
            foreach (DataRow row in dataTable.Rows)
            {
                var rowValues = new List<string>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    rowValues.Add($"{col.ColumnName}: {row[col]}");
                }
                sb.AppendLine(string.Join(" | ", rowValues));
            }

            return sb.ToString();
        }

        public async Task<string> GetTableSchemaAsync(string tableName)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var schema = connection.GetSchema("Columns", new[] { null, null, tableName });
                return schema.ToString();
            }
        }
    }
}

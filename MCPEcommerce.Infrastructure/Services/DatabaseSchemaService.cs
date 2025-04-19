using System.Text;
using MCPEcommerce.Core.Interfaces;
using MySql.Data.MySqlClient;

namespace MCPEcommerce.Infrastructure.Services
{
    public class DatabaseSchemaService : IDatabaseSchemaService
    {
        private readonly string _connectionString;

        public DatabaseSchemaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<string> GetDatabaseSchemaAsync()
        {
            var schema = new StringBuilder();
            schema.AppendLine("Database structure:\n");

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get all tables
                var tables = await GetTablesAsync(connection);
                foreach (var table in tables)
                {
                    schema.AppendLine($"Table: {table}");

                    // Get table columns
                    var columns = await GetColumnsAsync(connection, table);
                    foreach (var column in columns)
                    {
                        schema.AppendLine($"- {column}");
                    }

                    // Get foreign keys
                    var foreignKeys = await GetForeignKeysAsync(connection, table);
                    if (foreignKeys.Any())
                    {
                        schema.AppendLine("\nRelationships:");
                        foreach (var fk in foreignKeys)
                        {
                            schema.AppendLine($"- {fk}");
                        }
                    }

                    schema.AppendLine("");
                }
            }

            schema.AppendLine("Important notes:");
            schema.AppendLine("1. Dates in MySQL use the format 'YYYY-MM-DD'");

            return schema.ToString();
        }

        private async Task<IEnumerable<string>> GetTablesAsync(MySqlConnection connection)
        {
            var tables = new List<string>();
            var command = new MySqlCommand(
                "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE()",
                connection
            );

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            return tables;
        }

        private async Task<IEnumerable<string>> GetColumnsAsync(
            MySqlConnection connection,
            string tableName
        )
        {
            var columns = new List<string>();
            var command = new MySqlCommand(
                @"SELECT 
                    COLUMN_NAME,
                    DATA_TYPE,
                    IS_NULLABLE,
                    COLUMN_KEY,
                    COLUMN_COMMENT
                FROM information_schema.COLUMNS 
                WHERE TABLE_SCHEMA = DATABASE() 
                AND TABLE_NAME = @tableName",
                connection
            );

            command.Parameters.AddWithValue("@tableName", tableName);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(0);
                    var dataType = reader.GetString(1);
                    var isNullable = reader.GetString(2);
                    var columnKey = reader.GetString(3);
                    var comment = reader.GetString(4);

                    var columnInfo = $"{columnName} ({dataType})";
                    if (columnKey == "PRI")
                        columnInfo += ", PK";
                    if (columnKey == "MUL")
                        columnInfo += ", FK";
                    if (isNullable == "NO")
                        columnInfo += ", NOT NULL";
                    if (!string.IsNullOrEmpty(comment))
                        columnInfo += $": {comment}";

                    columns.Add(columnInfo);
                }
            }

            return columns;
        }

        private async Task<IEnumerable<string>> GetForeignKeysAsync(
            MySqlConnection connection,
            string tableName
        )
        {
            var foreignKeys = new List<string>();
            var command = new MySqlCommand(
                @"SELECT 
                    COLUMN_NAME,
                    REFERENCED_TABLE_NAME,
                    REFERENCED_COLUMN_NAME
                FROM information_schema.KEY_COLUMN_USAGE 
                WHERE TABLE_SCHEMA = DATABASE() 
                AND TABLE_NAME = @tableName 
                AND REFERENCED_TABLE_NAME IS NOT NULL",
                connection
            );

            command.Parameters.AddWithValue("@tableName", tableName);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(0);
                    var referencedTable = reader.GetString(1);
                    var referencedColumn = reader.GetString(2);

                    foreignKeys.Add(
                        $"{tableName}.{columnName} -> {referencedTable}.{referencedColumn}"
                    );
                }
            }

            return foreignKeys;
        }
    }
}

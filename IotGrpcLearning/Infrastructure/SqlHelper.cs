using IotGrpcLearning.Interfaces;
using Microsoft.Data.Sqlite;
using System.Text;

namespace IotGrpcLearning.Infrastructure;

/// <summary>
/// Provides SQL utility methods for common database operations.
/// </summary>
public sealed class SqlHelper : ISqlHelper
{
    public async Task<string?> GetPropertyTableAsync(
        SqliteConnection conn,
        CancellationToken ct,
        string table,
        string column,
        string columnValue,
        string getColumn)
    {
        // Validate identifiers
        var safeTable = IdentifierSanitizer.QuoteIdentifier(table);
        var safeColumn = IdentifierSanitizer.QuoteIdentifier(column);
        var safeGetColumn = IdentifierSanitizer.QuoteIdentifier(getColumn);

        using var sel = conn.CreateCommand();
        sel.CommandText = $"SELECT {safeGetColumn} FROM {safeTable} WHERE {safeColumn} = @columnValue LIMIT 1;";
        sel.Parameters.AddWithValue("@columnValue", columnValue);

        var scalar = await sel.ExecuteScalarAsync(ct);
        if (scalar != null && scalar != DBNull.Value)
            return Convert.ToString(scalar);
        return null;
    }

    public async Task<int> GetTotalCountFromTable(SqliteConnection conn, CancellationToken ct, string table)
    {
        var safeTable = IdentifierSanitizer.QuoteIdentifier(table);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM {safeTable};";

        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public (string filterQuery, List<SqliteParameter> parameters) BuildFilterQuery(
        string tableName,
        Dictionary<string, string[]> filters)
    {
        if (filters == null || filters.Count == 0)
        {
            return (string.Empty, new List<SqliteParameter>());
        }

        var parameters = new List<SqliteParameter>();
        var conditions = new List<string>();
        int paramIndex = 0;

        foreach (var filter in filters)
        {
            var columnName = IdentifierSanitizer.ValidateIdentifier(filter.Key, nameof(filter.Key));
            var filterValues = filter.Value;

            if (filterValues == null || filterValues.Length == 0)
                continue;

            var columnConditions = new List<string>();

            foreach (var value in filterValues)
            {
                var parameterName = $"@p{paramIndex}";
                columnConditions.Add($"\"{columnName}\" LIKE {parameterName}");
                parameters.Add(new SqliteParameter(parameterName, $"%{value}%"));
                paramIndex++;
            }

            if (columnConditions.Count > 0)
            {
                // Group conditions for the same column with OR
                conditions.Add($"({string.Join(" OR ", columnConditions)})");
            }
        }

        if (conditions.Count == 0)
            return (string.Empty, new List<SqliteParameter>());

        // Combine all column conditions with AND
        string filterQuery = " WHERE " + string.Join(" AND ", conditions);

        return (filterQuery, parameters);
    }

    public async Task<int> GetTotalCountWithConditions(
        SqliteConnection conn,
        CancellationToken ct,
        string table,
        Dictionary<string, string[]> filters)
    {
        var safeTable = IdentifierSanitizer.QuoteIdentifier(table);

        using var cmd = conn.CreateCommand();
        var queryBuilder = new StringBuilder($"SELECT COUNT(*) FROM {safeTable}");

        // Build the filter query and add it to the command
        var (filterQuery, parameters) = BuildFilterQuery(table, filters);

        queryBuilder.Append(filterQuery);

        // Add parameters to the command
        foreach (var parameter in parameters)
        {
            cmd.Parameters.Add(parameter);
        }

        cmd.CommandText = queryBuilder.ToString();

        // Use ExecuteScalarAsync instead of ExecuteReaderAsync
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }
}
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Interfaces;

/// <summary>
/// Provides SQL utility methods for common database operations.
/// </summary>
public interface ISqlHelper
{
    /// <summary>
    /// Retrieves a single property value from a table based on a column filter.
    /// </summary>
    Task<string?> GetPropertyTableAsync(
        SqliteConnection conn,
        CancellationToken ct,
        string table,
        string column,
        string columnValue,
        string getColumn);

    /// <summary>
    /// Gets the total count of rows in a table.
    /// </summary>
    Task<int> GetTotalCountFromTable(SqliteConnection conn, CancellationToken ct, string table);

    /// <summary>
    /// Builds a WHERE clause with LIKE conditions for filtering and returns parameterized query parts.
    /// </summary>
    (string filterQuery, List<SqliteParameter> parameters) BuildFilterQuery(
        string tableName,
        Dictionary<string, string[]> filters);

    /// <summary>
    /// Gets the total count of rows in a table with filter conditions applied.
    /// </summary>
    Task<int> GetTotalCountWithConditions(
        SqliteConnection conn,
        CancellationToken ct,
        string table,
        Dictionary<string, string[]> filters);
}
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Interfaces
{
    public interface IHelper
    {
        Task<string?> GetPropertyTableAsync(SqliteConnection conn, CancellationToken ct, string table, string column, string columnValue, string getColumn);
        Task<int> GetTotalCountFromTable(SqliteConnection conn, CancellationToken ct, string table);
        (string filterQuery, List<SqliteParameter> parameters) BuildFilterQuery(string tableName, Dictionary<string, string[]> filters);
        Task<int> GetTotalCountWithConditions(SqliteConnection conn, CancellationToken ct, string table, Dictionary<string, string[]> filters);

	}
}

using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Infrastructure
{
    public class Helper
    {
		public async Task<string?> GetPropertyTableAsync(SqliteConnection conn, CancellationToken ct, string table, string column, string columnValue, string getColumn)
		{
			// try select
			using var sel = conn.CreateCommand();
			sel.CommandText = $"SELECT {getColumn} FROM \"{table}\" WHERE {column} = @{column} LIMIT 1;";
			var pSel = sel.CreateParameter();
			pSel.ParameterName = $"@{column}";
			pSel.Value = columnValue;
			sel.Parameters.Add(pSel);

			var scalar = await sel.ExecuteScalarAsync(ct);
			if (scalar != null && scalar != DBNull.Value)
				return Convert.ToString(scalar);
			return null;
		}
	}
}

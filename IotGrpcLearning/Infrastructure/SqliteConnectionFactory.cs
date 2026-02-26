using System;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Infrastructure
{
	public interface ISqliteConnectionFactory
	{
		SqliteConnection CreateConnection();
	}

	public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
	{
		private readonly string _connectionString;

		public SqliteConnectionFactory(string connectionString)
		{
			_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
		}

		// returns a new unopened connection; caller should Open()/Dispose()
		public SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);
	}

	public static class SqliteExtensions
	{
		/// <summary>
		/// Validates the configured Sqlite Data Source file (throws if missing) and registers ISqliteConnectionFactory.
		/// Must be called before builder.Build().
		/// </summary>
		public static WebApplicationBuilder AddValidatedSqlite(this WebApplicationBuilder builder, string configKey = "ConnectionStrings:Sqlite", string defaultFileName = "factory.db")
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			// Read connection string from configuration sources
			var sqliteConnStr = builder.Configuration.GetConnectionString("Sqlite")
				?? builder.Configuration[configKey];

			// If not configured, fall back to a deterministic file in content root
			if (string.IsNullOrWhiteSpace(sqliteConnStr))
			{
				var sqliteFullPath = Path.Combine(builder.Environment.ContentRootPath, defaultFileName);
				sqliteConnStr = $"Data Source={sqliteFullPath}";
			}

			// Extract Data Source path (case-insensitive)
			const string dsPrefix = "Data Source=";
			var idx = sqliteConnStr.IndexOf(dsPrefix, StringComparison.OrdinalIgnoreCase);
			if (idx < 0)
			{
				throw new InvalidOperationException($"Connection string must contain '{dsPrefix}'. Value: {sqliteConnStr}");
			}

			var dataSource = sqliteConnStr.Substring(idx + dsPrefix.Length).Trim().Trim('"');

			// Resolve relative paths against content root
			if (!Path.IsPathRooted(dataSource))
			{
				dataSource = Path.Combine(builder.Environment.ContentRootPath, dataSource);
			}

			// Fail early if file is missing (avoids silently creating an empty DB)
			if (!File.Exists(dataSource))
			{
				throw new FileNotFoundException(
					$"SQLite database file not found at '{dataSource}'. Update '{configKey}' to point to your DB (for example 'Data Source=D:\\\\SQLite\\\\your.db').",
					dataSource);
			}

			// Register connection factory singleton (factory receives the exact connection string)
			builder.Services.AddSingleton<ISqliteConnectionFactory>(_ => new SqliteConnectionFactory(sqliteConnStr));

			return builder;
		}
	}
}
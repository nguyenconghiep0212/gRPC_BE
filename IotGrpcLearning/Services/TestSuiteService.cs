using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using System.Reflection.PortableExecutable;

namespace IotGrpcLearning.Services
{
    public sealed class TestSuiteService : ITestSuite
    {
		private readonly ISqliteConnectionFactory _dbFactory;
		public TestSuiteService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<TestSuiteDto> CreateAsync(TestSuiteDto dto, CancellationToken ct = default)
        {
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO TestSuite (name, machine, path, detail) " +
				"VALUES (@name, @machine, @path, @detail); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@machine", dto.MachineId);
			cmd.Parameters.AddWithValue("@path", dto.Path ?? string.Empty);
			cmd.Parameters.AddWithValue("@detail", dto.Detail ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new TestSuiteDto(newId, dto.Name ?? string.Empty, dto.MachineId, dto.Path ?? string.Empty, dto.Detail ?? string.Empty);
		}

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        { 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM TestSuite WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public async Task<IEnumerable<TestSuiteDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        {
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name, machine, path, detail FROM TestSuite LIMIT {body.limit} OFFSET {body.offset};";
			var roles = new List<TestSuiteDto>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				var machine = reader.GetInt32(2);
				var path = reader.GetString(3);
				var detail = reader.GetString(4);
				roles.Add(new TestSuiteDto(id, name, machine, path, detail));
			}
			return roles;
		}

        public async Task<TestSuiteDto?> GetAsync(int gid, CancellationToken ct = default)
        {
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name, machine, path, detail FROM TestSuite WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", gid);

			using var reader = await cmd.ExecuteReaderAsync(ct);
			if (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				var machine = reader.GetInt32(2);
				var path = reader.GetString(3);
				var detail = reader.GetString(4);
				return new TestSuiteDto(id, name, machine, path, detail);
			}

			return null;
		}

        public async Task<bool> UpdateAsync(int id, TestSuiteDto dto, CancellationToken ct = default)
        {
			 

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE TestSuite SET name = @name, machine = @machine, path = @path, detail = @detail WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@machine", dto.MachineId);
			cmd.Parameters.AddWithValue("@path", dto.Path ?? string.Empty);
			cmd.Parameters.AddWithValue("@detail", dto.Detail ?? string.Empty);
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}
    }
}

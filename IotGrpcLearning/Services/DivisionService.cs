using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Services
{
    public sealed class DivisionService : IDivision
    {
		private readonly ISqliteConnectionFactory _dbFactory;
		public DivisionService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<DivisionsDto> CreateAsync(DivisionsDto dto, CancellationToken ct = default)
        {
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Divisions (name) " +
				"VALUES (@name); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new DivisionsDto(newId, dto.Name ?? string.Empty);
		}

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
		 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Divisions WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public async Task<IEnumerable<DivisionsDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        {
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name FROM Divisions LIMIT {body.limit} OFFSET {body.offset};";
			var divisions = new List<DivisionsDto>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				divisions.Add(new DivisionsDto(id, name));
			}
			return divisions;
		}

        public async Task<DivisionsDto?> GetAsync(int id, CancellationToken ct = default)
        {
		 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name FROM Divisions WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id",id);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				static T GetSafe<T>(SqliteDataReader r, int i, Func<object, T> conv, T @default = default!)
				{
					if (r.IsDBNull(i)) return @default!;
					return conv(r.GetValue(i));
				}
				int division_id = GetSafe(rdr, 0, o => Convert.ToInt32(o));
				string division_name = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
				return new DivisionsDto(division_id, division_name); ;
			}

			return null;
		}

        public async Task<bool> UpdateAsync(int id, DivisionsDto dto, CancellationToken ct = default)
        { 
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Divisions SET name = @name WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}
    }
}

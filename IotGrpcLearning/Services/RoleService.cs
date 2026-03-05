using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Services
{
    public sealed class RoleService : IRole
    {
		private readonly ISqliteConnectionFactory _dbFactory;
		public RoleService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<RolesDto> CreateAsync(RolesDto dto, CancellationToken ct = default)
        {
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Roles (name) " +
				"VALUES (@name); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new RolesDto(newId, dto.Name ?? string.Empty);
		}

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        { 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Roles WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public async Task<IEnumerable<RolesDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        {
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name FROM Roles LIMIT {body.limit} OFFSET {body.offset};";
			var roles = new List<RolesDto>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				roles.Add(new RolesDto(id, name));
			}
			return roles;
		}

        public async Task<RolesDto?> GetAsync(int id, CancellationToken ct = default)
        {
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name FROM Roles WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", id);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				static T GetSafe<T>(SqliteDataReader r, int i, Func<object, T> conv, T @default = default!)
				{
					if (r.IsDBNull(i)) return @default!;
					return conv(r.GetValue(i));
				}
				int role_id = GetSafe(rdr, 0, o => Convert.ToInt32(o));
				string role_name = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
				return new RolesDto(role_id, role_name); ;
			}

			return null;
		}

        public async Task<bool> UpdateAsync(int id, RolesDto dto, CancellationToken ct = default)
        {
			 

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Roles SET name = @name WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}
    }
}

using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Services
{
    public sealed class SiteService : ISite
    {
		private readonly ISqliteConnectionFactory _dbFactory;
		public SiteService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<SitesDto> CreateAsync(SitesDto dto, CancellationToken ct = default)
        {
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Sites (name, location, address) " +
				"VALUES (@name, @location, @address); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@location", dto.Location ?? string.Empty);
			cmd.Parameters.AddWithValue("@address", dto.Address ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new SitesDto(newId, dto.Name ?? string.Empty, dto.Location ?? string.Empty, dto.Address ?? string.Empty);
		}

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        { 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Sites WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public async Task<IEnumerable<SitesDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        {
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name FROM Sites LIMIT {body.limit} OFFSET {body.offset};";
			var roles = new List<SitesDto>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				var location = reader.GetString(1);
				var address = reader.GetString(1);
				roles.Add(new SitesDto(id, name, location, address));
			}
			return roles;
		}

        public async Task<SitesDto?> GetAsync(int id, CancellationToken ct = default)
        {
		 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name, location, address FROM Sites WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", id);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				static T GetSafe<T>(SqliteDataReader r, int i, Func<object, T> conv, T @default = default!)
				{
					if (r.IsDBNull(i)) return @default!;
					return conv(r.GetValue(i));
				}
				int site_id = GetSafe(rdr, 0, o => Convert.ToInt32(o));
				string site_name = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
				string site_location = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
				string site_address = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
				return new SitesDto(site_id, site_name, site_location, site_address);
			}

			return null;
		}

        public async Task<bool> UpdateAsync(int id, SitesDto dto, CancellationToken ct = default)
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

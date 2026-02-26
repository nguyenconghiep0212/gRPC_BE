using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;

namespace IotGrpcLearning.Services
{
	public sealed class VendorService: IVendor
	{
		private readonly ISqliteConnectionFactory _dbFactory;
		public VendorService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<VendorDto> CreateAsync(VendorDto dto, CancellationToken ct = default)
		{
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Vendors (name) " +
				"VALUES (@name); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new VendorDto(newId, dto.Name ?? string.Empty);
		}
		public async Task<IEnumerable<VendorDto>> GetAllAsync(CancellationToken ct = default)
		{
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name FROM Vendors;";
			var vendors = new List<VendorDto>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				vendors.Add(new VendorDto(id, name));
			}
			return vendors;
		}

		public async Task<bool> UpdateAsync(string id, VendorDto dto, CancellationToken ct = default)
		{
			if (!int.TryParse(id, out var parsedId))
				return false;

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Vendors SET name = @name WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);  
			cmd.Parameters.AddWithValue("@id", parsedId);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
		{
			if (!int.TryParse(id, out var parsedId))
				return false;

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Vendors WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", parsedId);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public Task<VendorDto?> GetAsync(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}

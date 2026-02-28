using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Services
{
	public sealed class MachineService : IMachineService
	{
		private readonly ISqliteConnectionFactory _dbFactory;
		public MachineService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<MachineDto> CreateAsync(MachineDto dto, CancellationToken ct = default)
		{
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Machines (  name, alias, details, vendor, purchase_price, purchase_date, site) " +
				"VALUES ( @name, @alias, @details, @vendor, @price, @pdate, @site); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@alias", dto.Alias ?? string.Empty);
			cmd.Parameters.AddWithValue("@details", dto.Details ?? string.Empty);
			cmd.Parameters.AddWithValue("@vendor", dto.Vendor);
			cmd.Parameters.AddWithValue("@price", dto.PurchasePrice);

			if (dto.PurchaseDate == DateTime.MinValue)
				cmd.Parameters.AddWithValue("@pdate", DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@pdate", dto.PurchaseDate.ToString("o"));

			cmd.Parameters.AddWithValue("@site", dto.Site);

			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new MachineDto(newId, dto.Name ?? string.Empty, dto.Alias ?? string.Empty, dto.Details ?? string.Empty,
				dto.Vendor, dto.PurchasePrice, dto.PurchaseDate,  dto.Site);

		}

		public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
		{
			 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Machines WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		public async Task<IEnumerable<MachineDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
		{
			var list = new List<MachineDto>();
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name, alias, details, vendor, purchase_price, purchase_date, site FROM Machines ORDER BY id LIMIT {body.limit} OFFSET {body.offset};";
			using var rdr = await cmd.ExecuteReaderAsync(ct);

			while (await rdr.ReadAsync(ct))
			{
				list.Add(ReadMachine(rdr));
			}

			return list;
		}

		public async Task<MachineDto?> GetAsync(int id, CancellationToken ct = default)
		{
		 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name, alias, details, vendor, purchase_price, purchase_date, site FROM Machines WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", id);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				return ReadMachine(rdr);
			}

			return null;
		}

		public async Task<bool> UpdateAsync(int id, MachineDto dto, CancellationToken ct = default)
		{ 

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Machines SET name = @name, alias = @alias, details = @details, vendor = @vendor, " +
				"purchase_price = @price, purchase_date = @pdate, site = @site WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@alias", dto.Alias ?? string.Empty);
			cmd.Parameters.AddWithValue("@details", dto.Details ?? string.Empty);
			cmd.Parameters.AddWithValue("@vendor", dto.Vendor);
			cmd.Parameters.AddWithValue("@price", dto.PurchasePrice);

			if (dto.PurchaseDate == DateTime.MinValue)
				cmd.Parameters.AddWithValue("@pdate", DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@pdate", dto.PurchaseDate.ToString("o"));

			cmd.Parameters.AddWithValue("@site", dto.Site);
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		#region ===== HELPER ======
		private static MachineDto ReadMachine(SqliteDataReader rdr)
		{
			static T GetSafe<T>(SqliteDataReader r, int i, Func<object, T> conv, T @default = default!)
			{
				if (r.IsDBNull(i)) return @default!;
				return conv(r.GetValue(i));
			}

			int id = GetSafe(rdr, 0, o => Convert.ToInt32(o));
			string name = GetSafe(rdr, 1, o => Convert.ToString(o) ?? string.Empty, string.Empty);
			string alias = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
			string details = GetSafe(rdr, 3, o => Convert.ToString(o) ?? string.Empty, string.Empty);
			int vendor = GetSafe(rdr, 4, o => Convert.ToInt32(o), 0);
			double price = GetSafe(rdr, 5, o => Convert.ToDouble(o), 0.0);

			// purchase_date might be stored as TEXT or numeric; attempt to parse
			DateTime purchaseDate;
			if (!rdr.IsDBNull(6))
			{
				var val = rdr.GetValue(6);
				if (val is DateTime dt) purchaseDate = dt;
				else if (DateTime.TryParse(Convert.ToString(val), out var parsed)) purchaseDate = parsed;
				else purchaseDate = DateTime.MinValue;
			}
			else
			{
				purchaseDate = DateTime.MinValue;
			}

			int site = GetSafe(rdr, 7, o => Convert.ToInt32(o), 0);

			return new MachineDto(id, name, alias, details, vendor, price, purchaseDate, site);
		}
		#endregion
	}
}

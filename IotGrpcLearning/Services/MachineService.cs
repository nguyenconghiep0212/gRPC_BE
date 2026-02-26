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
				"INSERT INTO Machines (machine_info_id, name, details, vendor, purchase_price, purchase_date, status, site) " +
				"VALUES (@mi, @name, @details, @vendor, @price, @pdate, @status, @site); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@mi", dto.MachineInfoId);
			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@details", dto.Details ?? string.Empty);
			cmd.Parameters.AddWithValue("@vendor", dto.Vendor);
			cmd.Parameters.AddWithValue("@price", dto.PurchasePrice);

			if (dto.PurchaseDate == DateTime.MinValue)
				cmd.Parameters.AddWithValue("@pdate", DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@pdate", dto.PurchaseDate.ToString("o"));

			cmd.Parameters.AddWithValue("@status", dto.Status);
			cmd.Parameters.AddWithValue("@site", dto.Site);

			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new MachineDto(newId, dto.MachineInfoId, dto.Name ?? string.Empty, dto.Details ?? string.Empty,
				dto.Vendor, dto.PurchasePrice, dto.PurchaseDate, dto.Status, dto.Site);

		}

		public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
		{
			if (!int.TryParse(id, out var parsedId))
				return false;

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Machines WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", parsedId);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		public async Task<IEnumerable<MachineDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = new List<MachineDto>();
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, machine_info_id, name, details, vendor, purchase_price, purchase_date, status, site FROM Machines ORDER BY id;";
			using var rdr = await cmd.ExecuteReaderAsync(ct);

			while (await rdr.ReadAsync(ct))
			{
				list.Add(ReadMachine(rdr));
			}

			return list;
		}

		public async Task<MachineDto?> GetAsync(string id, CancellationToken ct = default)
		{
			if (!int.TryParse(id, out var parsedId))
				return null;

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, machine_info_id, name, details, vendor, purchase_price, purchase_date, status, site FROM Machines WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", parsedId);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				return ReadMachine(rdr);
			}

			return null;
		}

		public async Task<bool> UpdateAsync(string id, MachineDto dto, CancellationToken ct = default)
		{
			if (!int.TryParse(id, out var parsedId))
				return false;

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Machines SET machine_info_id = @mi, name = @name, details = @details, vendor = @vendor, " +
				"purchase_price = @price, purchase_date = @pdate, status = @status, site = @site WHERE id = @id;";

			cmd.Parameters.AddWithValue("@mi", dto.MachineInfoId);
			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@details", dto.Details ?? string.Empty);
			cmd.Parameters.AddWithValue("@vendor", dto.Vendor);
			cmd.Parameters.AddWithValue("@price", dto.PurchasePrice);

			if (dto.PurchaseDate == DateTime.MinValue)
				cmd.Parameters.AddWithValue("@pdate", DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@pdate", dto.PurchaseDate.ToString("o"));

			cmd.Parameters.AddWithValue("@status", dto.Status);
			cmd.Parameters.AddWithValue("@site", dto.Site);
			cmd.Parameters.AddWithValue("@id", parsedId);

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
			int mi = GetSafe(rdr, 1, o => Convert.ToInt32(o), 0);
			string name = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
			string details = GetSafe(rdr, 3, o => Convert.ToString(o) ?? string.Empty, string.Empty);
			int vendor = GetSafe(rdr, 4, o => Convert.ToInt32(o), 0);
			decimal price = GetSafe(rdr, 5, o => Convert.ToDecimal(o), 0m);

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

			int status = GetSafe(rdr, 7, o => Convert.ToInt32(o), 0);
			int site = GetSafe(rdr, 8, o => Convert.ToInt32(o), 0);

			return new MachineDto(id, mi, name, details, vendor, price, purchaseDate, status, site);
		}
		#endregion
	}
}

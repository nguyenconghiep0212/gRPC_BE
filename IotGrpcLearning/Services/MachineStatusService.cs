using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;

namespace IotGrpcLearning.Services
{
	public sealed class MachineStatusService : IMachineStatusService
	{
		private readonly ISqliteConnectionFactory _dbFactory;
		public MachineStatusService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<MachineStatusDto> CreateAsync(MachineStatusDto dto, CancellationToken ct = default)
		{
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO MachineStatus ( machine, health, is_online, last_online) " +
				"VALUES ( @machine, @health, @is_online, @last_online); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@machine", dto.MachineId);
			cmd.Parameters.AddWithValue("@health", dto.Health ?? string.Empty);
			cmd.Parameters.AddWithValue("@is_online", dto.IsOnline);

			if (dto.LastOnline == DateTime.MinValue)
				cmd.Parameters.AddWithValue("@is_online", DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@is_online", dto.LastOnline.ToString("o"));
			 
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new MachineStatusDto(newId, dto.MachineId, dto.Health ?? string.Empty, dto.IsOnline, dto.LastOnline);

		}

		public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
		{
			 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM MachineStatus WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		public async Task<IEnumerable<MachineStatusDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
		{
			var list = new List<MachineStatusDto>();
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, machine, health, is_online, last_online FROM MachineStatus ORDER BY id LIMIT {body.limit} OFFSET {body.offset};";
			using var rdr = await cmd.ExecuteReaderAsync(ct);

			while (await rdr.ReadAsync(ct))
			{
				list.Add(ReadMachine(rdr));
			}

			return list;
		}

		public async Task<MachineStatusDto?> GetAsync(int id, CancellationToken ct = default)
		{
		 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, machine, health, is_online, last_online FROM MachineStatus WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", id);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				return ReadMachine(rdr);
			}

			return null;
		}

		public async Task<bool> UpdateAsync(int id, MachineStatusDto dto, CancellationToken ct = default)
		{ 

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE MachineStatus SET machine = @machine, health = @health, is_online = @is_online, last_online = @last_online WHERE id = @id;";

			cmd.Parameters.AddWithValue("@machine", dto.MachineId);
			cmd.Parameters.AddWithValue("@health", dto.Health ?? string.Empty);
			cmd.Parameters.AddWithValue("@is_online", dto.IsOnline);

			if (dto.LastOnline == DateTime.MinValue)
				cmd.Parameters.AddWithValue("@last_online", DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@last_online", dto.LastOnline.ToString("o"));

			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		#region ===== HELPER ======
		private static MachineStatusDto ReadMachine(SqliteDataReader rdr)
		{
			static T GetSafe<T>(SqliteDataReader r, int i, Func<object, T> conv, T @default = default!)
			{
				if (r.IsDBNull(i)) return @default!;
				return conv(r.GetValue(i));
			}

			int id = GetSafe(rdr, 0, o => Convert.ToInt32(o));
			int machine = GetSafe(rdr, 1, o => Convert.ToInt32(o), 0);
			string health = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
			bool is_online = GetSafe(rdr, 3, o => Convert.ToBoolean(o), false); 
			// purchase_date might be stored as TEXT or numeric; attempt to parse
			DateTime last_online;
			if (!rdr.IsDBNull(4))
			{
				var val = rdr.GetValue(4);
				if (val is DateTime dt) last_online = dt;
				else if (DateTime.TryParse(Convert.ToString(val), out var parsed)) last_online = parsed;
				else last_online = DateTime.MinValue;
			}
			else
			{
				last_online = DateTime.MinValue;
			}

			return new MachineStatusDto(id, machine, health, is_online, last_online);
		}
		#endregion
	}
}

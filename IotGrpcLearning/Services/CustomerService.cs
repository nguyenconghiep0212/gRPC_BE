using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Services
{
	public sealed class CustomerService: ICustomer
	{
		private readonly ISqliteConnectionFactory _dbFactory;
		public CustomerService(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken ct = default)
		{
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Customers (name) " +
				"VALUES (@name); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new CustomerDto(newId, dto.Name ?? string.Empty);
		}
		public async Task<IEnumerable<CustomerDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
		{
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name FROM Customers LIMIT {body.limit} OFFSET {body.offset};";
			var vendors = new List<CustomerDto>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				vendors.Add(new CustomerDto(id, name));
			}
			return vendors;
		}

		public async Task<bool> UpdateAsync(int id, CustomerDto dto, CancellationToken ct = default)
		{ 

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Customers SET name = @name WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);  
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
		{ 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Customers WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public async Task<CustomerDto?> GetAsync(int id, CancellationToken ct = default)
        { 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name FROM Customers WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", id);

			using var rdr = await cmd.ExecuteReaderAsync(ct);
			if (await rdr.ReadAsync(ct))
			{
				static T GetSafe<T>(SqliteDataReader r, int i, Func<object, T> conv, T @default = default!)
				{
					if (r.IsDBNull(i)) return @default!;
					return conv(r.GetValue(i));
				}
				int customer_id = GetSafe(rdr, 0, o => Convert.ToInt32(o));
				string customer_name = GetSafe(rdr, 2, o => Convert.ToString(o) ?? string.Empty, string.Empty);
				return new CustomerDto(customer_id, customer_name); ;
			}

			return null;
		}
    }
}

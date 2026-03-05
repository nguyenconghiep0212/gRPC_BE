using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;

namespace IotGrpcLearning.Services
{
	public sealed class ProjectService: IProject
	{
		private readonly ISqliteConnectionFactory _dbFactory;
		private readonly Helper _helper;
		public ProjectService(ISqliteConnectionFactory dbFactory)
		{
			_helper = new Helper();
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

		public async Task<ProjectDto> CreateAsync(ProjectDto dto, CancellationToken ct = default)
		{
			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO Projects (name, customers_id, site, detail) " +
				"VALUES (@name, @customers_id, @site, @detail); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
			cmd.Parameters.AddWithValue("@customers_id", dto.CustomerId);
			cmd.Parameters.AddWithValue("@site", dto.SiteId);
			cmd.Parameters.AddWithValue("@detail", dto.Name ?? string.Empty);
			var result = await cmd.ExecuteScalarAsync(ct);
			var newId = Convert.ToInt32(result);

			return new ProjectDto(newId, dto.Name ?? string.Empty, dto.CustomerId, dto.SiteId, dto.Details ?? string.Empty);
		}
		public async Task<IEnumerable<ProjectResponse>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
		{
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT id, name, customers_id, site, detail FROM Projects LIMIT {body.limit} OFFSET {body.offset};";
			var project = new List<ProjectResponse>();
			using var reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var name = reader.GetString(1);
				var customerId = reader.GetInt32(2);
				var siteId = reader.GetInt32(3);
				var detail = reader.GetString(4);

				string customer = await _helper.GetPropertyTableAsync(conn, ct, "Customers", "id", customerId.ToString(), "name") ?? string.Empty;
				string site = await _helper.GetPropertyTableAsync(conn, ct, "Sites", "id", siteId.ToString(), "name") ?? string.Empty;

				project.Add(new ProjectResponse(id, name, customerId, customer,	siteId, site, detail));
			}
			return project;
		}

		public async Task<bool> UpdateAsync(int id, ProjectDto dto, CancellationToken ct = default)
		{ 

			if (dto == null) throw new ArgumentNullException(nameof(dto));

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"UPDATE Projects SET name = @name, customers_id = @customer_id, site = @site, detail = @detail WHERE id = @id;";

			cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);  
			cmd.Parameters.AddWithValue("@customer_id", dto.CustomerId);  
			cmd.Parameters.AddWithValue("@site", dto.SiteId);  
			cmd.Parameters.AddWithValue("@detail", dto.Details ?? string.Empty);
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
		{ 

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Projects WHERE id = @id;";
			cmd.Parameters.AddWithValue("@id", id);

			var rows = await cmd.ExecuteNonQueryAsync(ct);
			return rows > 0;
		}

        public async Task<ProjectResponse?> GetAsync(int id, CancellationToken ct = default)
        { 
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id, name, customers_id, site, detail FROM Projects WHERE id = @id LIMIT 1;";
			cmd.Parameters.AddWithValue("@id", id);

			using var reader = await cmd.ExecuteReaderAsync(ct);
			if (await reader.ReadAsync(ct))
			{
				var project_id = reader.GetInt32(0);
				var name = reader.GetString(1);
				var customerId = reader.GetInt32(2);
				var siteId = reader.GetInt32(3);
				var detail = reader.GetString(4);

				string customer = await _helper.GetPropertyTableAsync(conn, ct, "Customers", "id", customerId.ToString(), "name") ?? string.Empty;
				string site = await _helper.GetPropertyTableAsync(conn, ct, "Sites", "id", siteId.ToString(), "name") ?? string.Empty;
				return (new ProjectResponse(project_id, name, customerId, customer, siteId, site, detail));
			}

			return null;
		}
    }
}

using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Proto;
using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Linq;

namespace IotGrpcLearning.Services
{
	public sealed class ProjectService : IProject
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
		public async Task<ListDto<ProjectResponse>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
		{
			string tableName = "Projects";
			var list = new List<ProjectResponse>();
			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);
			using var cmd = conn.CreateCommand();
			// Start building the base query
			var queryBuilder = new StringBuilder($"SELECT id, name, customers_id, site, detail FROM {tableName}");
			if (body.filters != null)
			{
				// Call the BuildFilterQuery method
				var (filterQuery, parameters) = _helper.BuildFilterQuery(tableName, body.filters);
				if (!string.IsNullOrEmpty(filterQuery))
				{
					queryBuilder.Append(filterQuery);
				}
				if (parameters.Count > 0)
				{
					foreach (var parameter in parameters)
					{
						cmd.Parameters.Add(parameter);
					}
				}
			}
			// Adding pagination and ordering
			if (body.limit != null)
			{
				queryBuilder.Append($" ORDER BY id LIMIT {body.limit} ");
			}
			if (body.offset != null)
			{
				queryBuilder.Append($" OFFSET {body.offset};");
			}
			cmd.CommandText = queryBuilder.ToString();
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

				list.Add(new ProjectResponse(id, name, customerId, customer, siteId, site, detail));
			}
			int total = await _helper.GetTotalCountWithConditions(conn, ct, "Projects", body.filters);
			ListDto<ProjectResponse> response = new ListDto<ProjectResponse>(list, total);
			return response;
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

		public async Task<List<ProjectMemberResponse>> GetProjectMembers(int projectId, CancellationToken ct)
		{
			EmployeeService _employeeService = new EmployeeService(_dbFactory);
			ProjectService _projectService = new ProjectService(_dbFactory);
			var list = new List<ProjectMemberResponse>();

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT pe.id, pe.employee_id " +
				"FROM ProjectEmployee pe " +
				"WHERE pe.project_id = @projectId;";

			cmd.Parameters.AddWithValue("@projectId", projectId);

			using var reader = await cmd.ExecuteReaderAsync();
			if (await reader.ReadAsync(ct))
			{
				var id = reader.GetInt32(0);
				var employeeId = reader.GetInt32(0);

				EmployeeResponse employeeDetail = await _employeeService.GetAsync(employeeId, ct);
				ProjectResponse projectDetail = await _projectService.GetAsync(projectId, ct);
				list.Add(new ProjectMemberResponse(id, projectDetail, employeeDetail));
			}

			return list;
		}

		public async Task<List<ProjectMemberDto>> AddMembersToProject(int projectId, int[] employeeIds, CancellationToken ct)
		{
			var list = new List<ProjectMemberDto>();

			using var conn = _dbFactory.CreateConnection();
			await conn.OpenAsync(ct);

			using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"INSERT INTO ProjectEmployee (project_id, employee_id) " +
				"VALUES (@project_id, @employee_id); " +
				"SELECT last_insert_rowid();";

			cmd.Parameters.AddWithValue("@project_id", projectId);

			foreach (var employeeId in employeeIds)
			{
				cmd.Parameters.AddWithValue("@employee_id", employeeId);

				var result = await cmd.ExecuteScalarAsync(ct);
				var newId = Convert.ToInt32(result);
				list.Add(new ProjectMemberDto(newId, projectId, employeeId));
			}

			return list;
		}
	}
}

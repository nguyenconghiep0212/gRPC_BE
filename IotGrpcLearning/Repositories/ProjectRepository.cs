using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System.Text;

namespace IotGrpcLearning.Repositories;

/// <summary>
/// Optimized repository for Project data access using JOINs.
/// </summary>
public sealed class ProjectRepository : IProjectRepository
{
    private readonly ISqliteConnectionFactory _dbFactory;
    private readonly ISqlHelper _sqlHelper;

    public ProjectRepository(ISqliteConnectionFactory dbFactory, ISqlHelper sqlHelper)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
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
        cmd.Parameters.AddWithValue("@detail", dto.Details ?? string.Empty);

        var result = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(result);

        return new ProjectDto(newId, dto.Name, dto.CustomerId, dto.SiteId, dto.Details);
    }

    public async Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                p.id, p.name, p.customers_id, c.name AS customer_name, 
                p.site, s.name AS site_name, p.detail
            FROM Projects p
            LEFT JOIN Customers c ON p.customers_id = c.id
            LEFT JOIN Sites s ON p.site = s.id
            WHERE p.id = @id
            LIMIT 1;";

        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToProjectResponse(reader);
        }

        return null;
    }

    public async Task<ListDto<ProjectResponse>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var list = new List<ProjectResponse>();

        var queryBuilder = new StringBuilder(@"
            SELECT 
                p.id, p.name, p.customers_id, c.name AS customer_name, 
                p.site, s.name AS site_name, p.detail
            FROM Projects p
            LEFT JOIN Customers c ON p.customers_id = c.id
            LEFT JOIN Sites s ON p.site = s.id");

        using var cmd = conn.CreateCommand();

        if (pagination.filters != null)
        {
            var (filterQuery, parameters) = _sqlHelper.BuildFilterQuery("Projects", pagination.filters);
            if (!string.IsNullOrEmpty(filterQuery))
            {
                queryBuilder.Append(filterQuery);
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
        }

        queryBuilder.Append($" ORDER BY p.id LIMIT @limit OFFSET @offset;");
        cmd.Parameters.AddWithValue("@limit", pagination.limit);
        cmd.Parameters.AddWithValue("@offset", pagination.offset);

        cmd.CommandText = queryBuilder.ToString();

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapToProjectResponse(reader));
        }

        int total = await _sqlHelper.GetTotalCountWithConditions(conn, ct, "Projects", pagination.filters);

        return new ListDto<ProjectResponse>(list, total);
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

    /// <summary>
    /// Gets all members of a project with optimized JOIN (fixes N+1 and bugs).
    /// </summary>
    public async Task<List<ProjectMemberResponse>> GetProjectMembersAsync(int projectId, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var list = new List<ProjectMemberResponse>();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                pe.id AS project_employee_id,
                pe.employee_id,
                e.avatar_url, e.name AS employee_name, e.email,
                e.role_id, r.name AS role_name,
                e.division_id, d.name AS division_name,
                e.supervisor, sup.name AS supervisor_name,
                e.site AS employee_site_id, es.name AS employee_site_name,
                p.id AS project_id, p.name AS project_name,
                p.customers_id, c.name AS customer_name,
                p.site AS project_site_id, ps.name AS project_site_name,
                p.detail
            FROM ProjectEmployee pe
            INNER JOIN Employees e ON pe.employee_id = e.id
            INNER JOIN Projects p ON pe.project_id = p.id
            LEFT JOIN Roles r ON e.role_id = r.id
            LEFT JOIN Divisions d ON e.division_id = d.id
            LEFT JOIN Employees sup ON e.supervisor = sup.id
            LEFT JOIN Sites es ON e.site = es.id
            LEFT JOIN Customers c ON p.customers_id = c.id
            LEFT JOIN Sites ps ON p.site = ps.id
            WHERE pe.project_id = @projectId
            ORDER BY pe.id;";

        cmd.Parameters.AddWithValue("@projectId", projectId);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        
        // ✅ FIXED: Using WHILE instead of IF
        while (await reader.ReadAsync(ct))
        {
            // ✅ FIXED: Correct column indices
            var projectEmployeeId = reader.GetInt32(0);
            var employeeId = reader.GetInt32(1);

            var avatarUrl = reader.GetString(2);
            var employeeName = reader.GetString(3);
            var email = reader.GetString(4);
            var roleId = reader.GetInt32(5);
            var roleName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
            var divisionId = reader.GetInt32(7);
            var divisionName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
            var supervisorId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);
            var supervisorName = reader.IsDBNull(10) ? null : reader.GetString(10);
            var employeeSiteId = reader.GetInt32(11);
            var employeeSiteName = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);

            var employeeResponse = new EmployeeResponse(
                employeeId, avatarUrl, employeeName, email,
                roleId, roleName, divisionId, divisionName,
                supervisorId, supervisorName, employeeSiteId, employeeSiteName);

            var projectIdValue = reader.GetInt32(13);
            var projectName = reader.GetString(14);
            var customerId = reader.GetInt32(15);
            var customerName = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
            var projectSiteId = reader.GetInt32(17);
            var projectSiteName = reader.IsDBNull(18) ? string.Empty : reader.GetString(18);
            var detail = reader.GetString(19);

            var projectResponse = new ProjectResponse(
                projectIdValue, projectName, customerId, customerName,
                projectSiteId, projectSiteName, detail);

            list.Add(new ProjectMemberResponse(projectEmployeeId, projectResponse, employeeResponse));
        }

        return list;
    }

    /// <summary>
    /// Adds multiple members to a project with transaction safety.
    /// </summary>
    public async Task<List<ProjectMemberDto>> AddProjectMembersAsync(int projectId, int[] employeeIds, CancellationToken ct = default)
    {
        if (employeeIds == null || employeeIds.Length == 0)
            return new List<ProjectMemberDto>();

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var list = new List<ProjectMemberDto>();

        using var transaction = conn.BeginTransaction();
        
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText =
                "INSERT INTO ProjectEmployee (project_id, employee_id) " +
                "VALUES (@project_id, @employee_id); " +
                "SELECT last_insert_rowid();";

            var projectIdParam = cmd.CreateParameter();
            projectIdParam.ParameterName = "@project_id";
            projectIdParam.Value = projectId;
            cmd.Parameters.Add(projectIdParam);

            var employeeIdParam = cmd.CreateParameter();
            employeeIdParam.ParameterName = "@employee_id";
            cmd.Parameters.Add(employeeIdParam);

            foreach (var employeeId in employeeIds)
            {
                employeeIdParam.Value = employeeId;

                var result = await cmd.ExecuteScalarAsync(ct);
                var newId = Convert.ToInt32(result);
                list.Add(new ProjectMemberDto(newId, projectId, employeeId));
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return list;
    }

    private static ProjectResponse MapToProjectResponse(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);
        var name = reader.GetString(1);
        var customerId = reader.GetInt32(2);
        var customerName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
        var siteId = reader.GetInt32(4);
        var siteName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        var detail = reader.GetString(6);

        return new ProjectResponse(id, name, customerId, customerName, siteId, siteName, detail);
    }
}
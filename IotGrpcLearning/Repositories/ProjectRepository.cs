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

        // Build query with JOIN to eliminate N+1
        var queryBuilder = new StringBuilder(@"
            SELECT 
                p.id, p.name, p.customers_id, c.name AS customer_name, 
                p.site, s.name AS site_name, p.detail
            FROM Projects p
            LEFT JOIN Customers c ON p.customers_id = c.id
            LEFT JOIN Sites s ON p.site = s.id");

        using var cmd = conn.CreateCommand();

        // Add filters if present
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

        // Add ordering and pagination
        queryBuilder.Append($" ORDER BY p.id LIMIT @limit OFFSET @offset;");
        cmd.Parameters.AddWithValue("@limit", pagination.limit);
        cmd.Parameters.AddWithValue("@offset", pagination.offset);

        cmd.CommandText = queryBuilder.ToString();

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapToProjectResponse(reader));
        }

        // Get total count
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
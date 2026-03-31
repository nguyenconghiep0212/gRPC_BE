using System.Numerics;
using System.Xml.Linq;
using Grpc.Core;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Services;

public sealed class EmployeeService : IEmployee
{
    private readonly ISqliteConnectionFactory _dbFactory;
    private readonly ISqlHelper _sqlHelper;

    public EmployeeService(ISqliteConnectionFactory dbFactory, ISqlHelper sqlHelper)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
    }

    public async Task<EmployeesDto> CreateAsync(EmployeesDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO Employees (avatar_url, name, email, role_id, division_id, supervisor, site) " +
            "VALUES (@avatar_url, @name, @email, @role_id, @division_id, @supervisor, @site); " +
            "SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@avatar_url", dto.AvatarUrl ?? string.Empty);
        cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
        cmd.Parameters.AddWithValue("@email", dto.Email ?? string.Empty);
        cmd.Parameters.AddWithValue("@role_id", dto.RoleId);
        cmd.Parameters.AddWithValue("@division_id", dto.DivisionId);
        cmd.Parameters.AddWithValue("@supervisor", (object?)dto.SupervisorId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@site", dto.SiteId);

        var result = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(result);

        return new EmployeesDto(newId, dto.AvatarUrl, dto.Name, dto.Email, dto.RoleId, dto.DivisionId, dto.SupervisorId, dto.SiteId);
    }

    public async Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                e.id, e.avatar_url, e.name, e.email,
                e.role_id, r.name AS role_name,
                e.division_id, d.name AS division_name,
                e.supervisor, sup.name AS supervisor_name,
                e.site, s.name AS site_name
            FROM Employees e
            LEFT JOIN Roles r ON e.role_id = r.id
            LEFT JOIN Divisions d ON e.division_id = d.id
            LEFT JOIN Employees sup ON e.supervisor = sup.id
            LEFT JOIN Sites s ON e.site = s.id
            WHERE e.id = @id
            LIMIT 1;";

        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToEmployeeResponse(reader);
        }

        return null;
    }

    public async Task<ListDto<EmployeeResponse>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var list = new List<EmployeeResponse>();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                e.id, e.avatar_url, e.name, e.email,
                e.role_id, r.name AS role_name,
                e.division_id, d.name AS division_name,
                e.supervisor, sup.name AS supervisor_name,
                e.site, s.name AS site_name
            FROM Employees e
            LEFT JOIN Roles r ON e.role_id = r.id
            LEFT JOIN Divisions d ON e.division_id = d.id
            LEFT JOIN Employees sup ON e.supervisor = sup.id
            LEFT JOIN Sites s ON e.site = s.id
            ORDER BY e.id
            LIMIT @limit OFFSET @offset;";

        cmd.Parameters.AddWithValue("@limit", pagination.limit);
        cmd.Parameters.AddWithValue("@offset", pagination.offset);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapToEmployeeResponse(reader));
        }

        int total = await _sqlHelper.GetTotalCountFromTable(conn, ct, "Employees");

        return new ListDto<EmployeeResponse>(list, total);
    }

    public async Task<bool> UpdateAsync(int id, EmployeesDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE Employees SET avatar_url = @avatar_url, name = @name, email = @email, " +
            "role_id = @role_id, division_id = @division_id, supervisor = @supervisor, site = @site " +
            "WHERE id = @id;";

        cmd.Parameters.AddWithValue("@avatar_url", dto.AvatarUrl ?? string.Empty);
        cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
        cmd.Parameters.AddWithValue("@email", dto.Email ?? string.Empty);
        cmd.Parameters.AddWithValue("@role_id", dto.RoleId);
        cmd.Parameters.AddWithValue("@division_id", dto.DivisionId);
        cmd.Parameters.AddWithValue("@supervisor", (object?)dto.SupervisorId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@site", dto.SiteId);
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Employees WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private static EmployeeResponse MapToEmployeeResponse(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);
        var avatarUrl = reader.GetString(1);
        var name = reader.GetString(2);
        var email = reader.GetString(3);
        var roleId = reader.GetInt32(4);
        var roleName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        var divisionId = reader.GetInt32(6);
        var divisionName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
        var supervisorId = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8);
        var supervisorName = reader.IsDBNull(9) ? null : reader.GetString(9);
        var siteId = reader.GetInt32(10);
        var siteName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);

        return new EmployeeResponse(id, avatarUrl, name, email, roleId, roleName,
            divisionId, divisionName, supervisorId, supervisorName, siteId, siteName);
    }

     
}

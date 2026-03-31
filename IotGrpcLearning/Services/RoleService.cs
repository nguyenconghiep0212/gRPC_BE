using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;

namespace IotGrpcLearning.Services;

public sealed class RoleService : IRole
{
    private readonly ISqliteConnectionFactory _dbFactory;


    public RoleService(ISqliteConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<RolesDto> CreateAsync(RolesDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO Roles (name) VALUES (@name); SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
        var result = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(result);

        return new RolesDto(newId, dto.Name ?? string.Empty);
    }

    public async Task<RolesDto?> GetAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name FROM Roles WHERE id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            var roleId = reader.GetInt32(0);
            var name = reader.GetString(1);
            return new RolesDto(roleId, name);
        }

        return null;
    }

    public async Task<IEnumerable<RolesDto>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var roles = new List<RolesDto>();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name FROM Roles ORDER BY id LIMIT @limit OFFSET @offset;";
        cmd.Parameters.AddWithValue("@limit", pagination.limit);
        cmd.Parameters.AddWithValue("@offset", pagination.offset);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            roles.Add(new RolesDto(id, name));
        }

        return roles;
    }

    public async Task<bool> UpdateAsync(int id, RolesDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Roles SET name = @name WHERE id = @id;";

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
        cmd.CommandText = "DELETE FROM Roles WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}

using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;

namespace IotGrpcLearning.Repositories;

public sealed class MachineStatusRepository : IMachineStatusRepository
{
    private readonly ISqliteConnectionFactory _dbFactory;
    private readonly ISqlHelper _sqlHelper;

    public MachineStatusRepository(ISqliteConnectionFactory dbFactory, ISqlHelper sqlHelper)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
    }

    public async Task<MachineStatusDto> CreateAsync(MachineStatusDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO MachineStatus (machine, health, is_online, last_online) " +
            "VALUES (@machine, @health, @is_online, @last_online); " +
            "SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@machine", dto.MachineId);
        cmd.Parameters.AddWithValue("@health", dto.Health ?? string.Empty);
        cmd.Parameters.AddWithValue("@is_online", dto.IsOnline);
        cmd.Parameters.AddWithValue("@last_online", dto.LastOnline == DateTime.MinValue 
            ? DBNull.Value 
            : dto.LastOnline.ToString("o"));

        var result = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(result);

        return new MachineStatusDto(newId, dto.MachineId, dto.Health, dto.IsOnline, dto.LastOnline);
    }

    public async Task<MachineStatusDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, machine, health, is_online, last_online FROM MachineStatus WHERE id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToMachineStatus(reader);
        }

        return null;
    }

    public async Task<MachineStatusDto?> GetByMachineIdAsync(int machineId, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, machine, health, is_online, last_online FROM MachineStatus WHERE machine = @machine LIMIT 1;";
        cmd.Parameters.AddWithValue("@machine", machineId);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToMachineStatus(reader);
        }

        return null;
    }

    public async Task<ListDto<MachineStatusDto>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var list = new List<MachineStatusDto>();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, machine, health, is_online, last_online FROM MachineStatus ORDER BY id LIMIT @limit OFFSET @offset;";
        cmd.Parameters.AddWithValue("@limit", pagination.limit);
        cmd.Parameters.AddWithValue("@offset", pagination.offset);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapToMachineStatus(reader));
        }

        int total = await _sqlHelper.GetTotalCountFromTable(conn, ct, "MachineStatus");

        return new ListDto<MachineStatusDto>(list, total);
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
        cmd.Parameters.AddWithValue("@last_online", dto.LastOnline == DateTime.MinValue 
            ? DBNull.Value 
            : dto.LastOnline.ToString("o"));
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
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

    private static MachineStatusDto MapToMachineStatus(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);
        var machineId = reader.GetInt32(1);
        var health = reader.GetString(2);
        var isOnline = reader.GetBoolean(3);
        var lastOnline = reader.IsDBNull(4) 
            ? DateTime.MinValue 
            : DateTime.Parse(reader.GetString(4));

        return new MachineStatusDto(id, machineId, health, isOnline, lastOnline);
    }
}
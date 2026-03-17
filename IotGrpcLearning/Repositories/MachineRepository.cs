using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System.Text;

namespace IotGrpcLearning.Repositories;

/// <summary>
/// Optimized repository for Machine data access using JOINs.
/// </summary>
public sealed class MachineRepository : IMachineRepository
{
    private readonly ISqliteConnectionFactory _dbFactory;
    private readonly ISqlHelper _sqlHelper;

    public MachineRepository(ISqliteConnectionFactory dbFactory, ISqlHelper sqlHelper)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
    }

    public async Task<MachineDto> CreateAsync(MachineDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO Machines (name, alias, details, vendor, purchase_price, purchase_date, site) " +
            "VALUES (@name, @alias, @details, @vendor, @price, @pdate, @site); " +
            "SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
        cmd.Parameters.AddWithValue("@alias", dto.Alias ?? string.Empty);
        cmd.Parameters.AddWithValue("@details", dto.Details ?? string.Empty);
        cmd.Parameters.AddWithValue("@vendor", dto.Vendor);
        cmd.Parameters.AddWithValue("@price", dto.PurchasePrice);
        cmd.Parameters.AddWithValue("@pdate", dto.PurchaseDate == DateTime.MinValue 
            ? DBNull.Value 
            : dto.PurchaseDate.ToString("o"));
        cmd.Parameters.AddWithValue("@site", dto.Site);

        var result = await cmd.ExecuteScalarAsync(ct);
        var newId = Convert.ToInt32(result);

        return new MachineDto(newId, dto.Name, dto.Alias, dto.Details, dto.Vendor, dto.PurchasePrice, dto.PurchaseDate, dto.Site);
    }

    public async Task<MachineResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                m.id, m.name, m.alias, m.details,
                m.vendor, v.name AS vendor_name,
                m.purchase_price, m.purchase_date,
                m.site, s.name AS site_name
            FROM Machines m
            LEFT JOIN Vendors v ON m.vendor = v.id
            LEFT JOIN Sites s ON m.site = s.id
            WHERE m.id = @id
            LIMIT 1;";

        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapToMachineResponse(reader);
        }

        return null;
    }

    public async Task<ListDto<MachineResponse>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        var list = new List<MachineResponse>();

        var queryBuilder = new StringBuilder(@"
            SELECT 
                m.id, m.name, m.alias, m.details,
                m.vendor, v.name AS vendor_name,
                m.purchase_price, m.purchase_date,
                m.site, s.name AS site_name
            FROM Machines m
            LEFT JOIN Vendors v ON m.vendor = v.id
            LEFT JOIN Sites s ON m.site = s.id");

        using var cmd = conn.CreateCommand();

        // Add filters if present
        if (pagination.filters != null)
        {
            var (filterQuery, parameters) = _sqlHelper.BuildFilterQuery("Machines", pagination.filters);
            if (!string.IsNullOrEmpty(filterQuery))
            {
                queryBuilder.Append(filterQuery);
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
        }

        queryBuilder.Append($" ORDER BY m.id LIMIT @limit OFFSET @offset;");
        cmd.Parameters.AddWithValue("@limit", pagination.limit);
        cmd.Parameters.AddWithValue("@offset", pagination.offset);

        cmd.CommandText = queryBuilder.ToString();

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapToMachineResponse(reader));
        }

        int total = await _sqlHelper.GetTotalCountWithConditions(conn, ct, "Machines", pagination.filters);

        return new ListDto<MachineResponse>(list, total);
    }

    public async Task<bool> UpdateAsync(int id, MachineDto dto, CancellationToken ct = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE Machines SET name = @name, alias = @alias, details = @details, vendor = @vendor, " +
            "purchase_price = @price, purchase_date = @pdate, site = @site WHERE id = @id;";

        cmd.Parameters.AddWithValue("@name", dto.Name ?? string.Empty);
        cmd.Parameters.AddWithValue("@alias", dto.Alias ?? string.Empty);
        cmd.Parameters.AddWithValue("@details", dto.Details ?? string.Empty);
        cmd.Parameters.AddWithValue("@vendor", dto.Vendor);
        cmd.Parameters.AddWithValue("@price", dto.PurchasePrice);
        cmd.Parameters.AddWithValue("@pdate", dto.PurchaseDate == DateTime.MinValue 
            ? DBNull.Value 
            : dto.PurchaseDate.ToString("o"));
        cmd.Parameters.AddWithValue("@site", dto.Site);
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Machines WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private static MachineResponse MapToMachineResponse(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);
        var name = reader.GetString(1);
        var alias = reader.GetString(2);
        var details = reader.GetString(3);
        var vendorId = reader.GetInt32(4);
        var vendorName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        var purchasePrice = reader.GetDouble(6);
        var purchaseDate = reader.IsDBNull(7) 
            ? DateTime.MinValue 
            : DateTime.Parse(reader.GetString(7));
        var siteId = reader.GetInt32(8);
        var siteName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);

        return new MachineResponse(id, name, alias, details, vendorId, vendorName, 
            purchasePrice, purchaseDate, siteId, siteName);
    }
}
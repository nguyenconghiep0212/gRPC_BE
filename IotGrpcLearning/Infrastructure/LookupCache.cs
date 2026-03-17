using IotGrpcLearning.Interfaces;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace IotGrpcLearning.Infrastructure;

/// <summary>
/// In-memory cache for frequently-accessed lookup tables.
/// Thread-safe and lazy-loaded.
/// </summary>
public sealed class LookupCache : ILookupCache
{
    private readonly ISqliteConnectionFactory _dbFactory;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    private ConcurrentDictionary<int, string>? _roles;
    private ConcurrentDictionary<int, string>? _sites;
    private ConcurrentDictionary<int, string>? _vendors;
    private ConcurrentDictionary<int, string>? _divisions;
    private ConcurrentDictionary<int, string>? _customers;
    
    private DateTime _lastRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(30);

    public LookupCache(ISqliteConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<string?> GetRoleNameAsync(int roleId, CancellationToken ct = default)
    {
        await EnsureCachedAsync(ct);
        return _roles?.TryGetValue(roleId, out var name) == true ? name : null;
    }

    public async Task<string?> GetSiteNameAsync(int siteId, CancellationToken ct = default)
    {
        await EnsureCachedAsync(ct);
        return _sites?.TryGetValue(siteId, out var name) == true ? name : null;
    }

    public async Task<string?> GetVendorNameAsync(int vendorId, CancellationToken ct = default)
    {
        await EnsureCachedAsync(ct);
        return _vendors?.TryGetValue(vendorId, out var name) == true ? name : null;
    }

    public async Task<string?> GetDivisionNameAsync(int divisionId, CancellationToken ct = default)
    {
        await EnsureCachedAsync(ct);
        return _divisions?.TryGetValue(divisionId, out var name) == true ? name : null;
    }

    public async Task<string?> GetCustomerNameAsync(int customerId, CancellationToken ct = default)
    {
        await EnsureCachedAsync(ct);
        return _customers?.TryGetValue(customerId, out var name) == true ? name : null;
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(ct);

            _roles = await LoadTableAsync(conn, "Roles", ct);
            _sites = await LoadTableAsync(conn, "Sites", ct);
            _vendors = await LoadTableAsync(conn, "Vendors", ct);
            _divisions = await LoadTableAsync(conn, "Divisions", ct);
            _customers = await LoadTableAsync(conn, "Customers", ct);

            _lastRefresh = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureCachedAsync(CancellationToken ct)
    {
        if (_roles == null || DateTime.UtcNow - _lastRefresh > _cacheLifetime)
        {
            await RefreshAsync(ct);
        }
    }

    private static async Task<ConcurrentDictionary<int, string>> LoadTableAsync(
        SqliteConnection conn,
        string tableName,
        CancellationToken ct)
    {
        var dict = new ConcurrentDictionary<int, string>();
        var safeTable = IdentifierSanitizer.QuoteIdentifier(tableName);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT id, name FROM {safeTable};";

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetInt32(0);
            var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            dict.TryAdd(id, name);
        }

        return dict;
    }
}
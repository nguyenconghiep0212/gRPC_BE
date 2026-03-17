namespace IotGrpcLearning.Interfaces;

/// <summary>
/// Provides cached access to frequently-used lookup tables.
/// </summary>
public interface ILookupCache
{
    /// <summary>
    /// Gets a role name by ID. Returns null if not found.
    /// </summary>
    Task<string?> GetRoleNameAsync(int roleId, CancellationToken ct = default);

    /// <summary>
    /// Gets a site name by ID. Returns null if not found.
    /// </summary>
    Task<string?> GetSiteNameAsync(int siteId, CancellationToken ct = default);

    /// <summary>
    /// Gets a vendor name by ID. Returns null if not found.
    /// </summary>
    Task<string?> GetVendorNameAsync(int vendorId, CancellationToken ct = default);

    /// <summary>
    /// Gets a division name by ID. Returns null if not found.
    /// </summary>
    Task<string?> GetDivisionNameAsync(int divisionId, CancellationToken ct = default);

    /// <summary>
    /// Gets a customer name by ID. Returns null if not found.
    /// </summary>
    Task<string?> GetCustomerNameAsync(int customerId, CancellationToken ct = default);

    /// <summary>
    /// Forces a refresh of all cached data.
    /// </summary>
    Task RefreshAsync(CancellationToken ct = default);
}
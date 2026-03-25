using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

/// <summary>
/// Repository for User data access.
/// </summary>
public interface IUserRepository
{
    Task<UserDto?> GetByEmployeeIdAsync(int employeeId, CancellationToken ct = default);
    Task<UserDto?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(UserDto user, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UserDto user, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
}
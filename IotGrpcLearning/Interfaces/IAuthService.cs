using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

/// <summary>
/// Service for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Validates a JWT token and returns user info.
    /// </summary>
    Task<UserDto?> ValidateTokenAsync(string token, CancellationToken ct = default);


    /// <summary>
    /// Changes user password.
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct = default);
}
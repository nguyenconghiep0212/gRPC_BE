namespace IotGrpcLearning.Models;

/// <summary>
/// User data model.
/// </summary>
public sealed record UserDto(
    int Id,
    int EmployeeId,
    string Username,
    string PasswordHash,
    string PasswordSalt,
    bool IsActive,
    DateTime LastLogin,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
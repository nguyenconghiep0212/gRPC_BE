namespace IotGrpcLearning.Models;

/// <summary>
/// Request model for user registration.
/// </summary>
public sealed record RegisterRequestDto(
    string Username,
    string Password,
    int EmployeeId,
    int RoleId
);
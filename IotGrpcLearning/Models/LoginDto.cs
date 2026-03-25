namespace IotGrpcLearning.Models;

/// <summary>
/// Request model for user login.
/// </summary>
public sealed record LoginRequestDto(
    string Username,
    string Password
);

public sealed record LoginResponseDto(
    int UserId,
    string Email,
    string Name,
    string Token,
    DateTime ExpiresAt
);
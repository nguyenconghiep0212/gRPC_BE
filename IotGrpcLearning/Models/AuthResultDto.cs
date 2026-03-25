namespace IotGrpcLearning.Models;

/// <summary>
/// Result of authentication operation.
/// </summary>
public sealed record AuthResultDto(
    bool Success,
    string? Message,
    LoginResponseDto? Data = null
);
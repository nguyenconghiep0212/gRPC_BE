namespace IotGrpcLearning.Models;

/// <summary>
/// Request model for changing password.
/// </summary>
public sealed record ChangePasswordRequestDto(
    string OldPassword,
    string NewPassword
);
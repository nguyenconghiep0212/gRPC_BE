namespace IotGrpcLearning.Models;

public record MachineStatusDto(
    int Id,
    int MachineId,
    string Health,
    bool IsOnline,
    DateTime LastOnline);
namespace IotGrpcLearning.Models;

public record MachineDto(
    int Id,
    int MachineInfoId,
    string Name,
    string Details,
    int Vendor,
    decimal PurchasePrice,
    DateTime PurchaseDate,
    int Status,
    int Site);
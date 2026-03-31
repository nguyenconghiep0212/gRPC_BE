using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IMachine
{
    Task<MachineDto> CreateAsync(MachineDto dto, CancellationToken ct = default);
    Task<MachineResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ListDto<MachineResponse>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, MachineDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
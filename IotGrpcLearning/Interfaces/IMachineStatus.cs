using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IMachineStatus
{
    Task<MachineStatusDto> CreateAsync(MachineStatusDto dto, CancellationToken ct = default);
    Task<MachineStatusDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MachineStatusDto?> GetByMachineIdAsync(int machineId, CancellationToken ct = default);
    Task<ListDto<MachineStatusDto>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, MachineStatusDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
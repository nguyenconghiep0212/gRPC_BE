using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IMachineStatusService
{
	Task<IEnumerable<MachineStatusDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default);
	Task<MachineStatusDto?> GetAsync(int id, CancellationToken ct = default);
	Task<MachineStatusDto> CreateAsync(MachineStatusDto dto, CancellationToken ct = default);
	Task<bool> UpdateAsync(int id, MachineStatusDto dto, CancellationToken ct = default);
	Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
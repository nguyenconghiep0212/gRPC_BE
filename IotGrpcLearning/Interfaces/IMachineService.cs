using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IMachineService
{
	Task<IEnumerable<MachineDto>> GetAllAsync(CancellationToken ct = default);
	Task<MachineDto?> GetAsync(string id, CancellationToken ct = default);
	Task<MachineDto> CreateAsync(MachineDto dto, CancellationToken ct = default);
	Task<bool> UpdateAsync(string id, MachineDto dto, CancellationToken ct = default);
	Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
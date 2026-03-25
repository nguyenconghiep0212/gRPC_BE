using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;

namespace IotGrpcLearning.Services;

public sealed class MachineStatusService : IMachineStatusService
{
    private readonly IMachineStatusRepository _repository;

    public MachineStatusService(IMachineStatusRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<MachineStatusDto> CreateAsync(MachineStatusDto dto, CancellationToken ct = default)
        => _repository.CreateAsync(dto, ct);

    public Task<MachineStatusDto?> GetAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<MachineStatusDto?> GetByMachineIdAsync(int machineId, CancellationToken ct = default)
        => _repository.GetByMachineIdAsync(machineId, ct);

    public Task<ListDto<MachineStatusDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        => _repository.GetAllAsync(body, ct);

    public Task<bool> UpdateAsync(int id, MachineStatusDto dto, CancellationToken ct = default)
        => _repository.UpdateAsync(id, dto, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}

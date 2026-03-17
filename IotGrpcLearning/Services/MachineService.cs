using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;

namespace IotGrpcLearning.Services;

public sealed class MachineService : IMachineService
{
    private readonly IMachineRepository _repository;

    public MachineService(IMachineRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<MachineDto> CreateAsync(MachineDto dto, CancellationToken ct = default)
        => _repository.CreateAsync(dto, ct);

    public Task<MachineResponse?> GetAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<ListDto<MachineResponse>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        => _repository.GetAllAsync(body, ct);

    public Task<bool> UpdateAsync(int id, MachineDto dto, CancellationToken ct = default)
        => _repository.UpdateAsync(id, dto, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}

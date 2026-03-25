using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;

namespace IotGrpcLearning.Services;

public sealed class RoleService : IRole
{
    private readonly IRoleRepository _repository;

    public RoleService(IRoleRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<RolesDto> CreateAsync(RolesDto dto, CancellationToken ct = default)
        => _repository.CreateAsync(dto, ct);

    public Task<RolesDto?> GetAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<IEnumerable<RolesDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        => _repository.GetAllAsync(body, ct);

    public Task<bool> UpdateAsync(int id, RolesDto dto, CancellationToken ct = default)
        => _repository.UpdateAsync(id, dto, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}

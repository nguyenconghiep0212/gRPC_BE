using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IRoleRepository
{
    Task<RolesDto> CreateAsync(RolesDto dto, CancellationToken ct = default);
    Task<RolesDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<RolesDto>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, RolesDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
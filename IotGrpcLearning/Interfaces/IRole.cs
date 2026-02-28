using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface IRole
    {
		Task<IEnumerable<RolesDto>> GetAllAsync(PaginationDto body,CancellationToken ct = default);
		Task<RolesDto?> GetAsync(int id, CancellationToken ct = default);
		Task<RolesDto> CreateAsync(RolesDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(int id, RolesDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(int id, CancellationToken ct = default);
	}
}

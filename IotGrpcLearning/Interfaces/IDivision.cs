using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface IDivision
    {
		Task<IEnumerable<DivisionsDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default);
		Task<DivisionsDto?> GetAsync(int id, CancellationToken ct = default);
		Task<DivisionsDto> CreateAsync(DivisionsDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(int id, DivisionsDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(int id, CancellationToken ct = default);
	}
}

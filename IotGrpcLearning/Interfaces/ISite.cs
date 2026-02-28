using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface ISite
    {
		Task<IEnumerable<SitesDto>> GetAllAsync(PaginationDto body,CancellationToken ct = default);
		Task<SitesDto?> GetAsync(int id, CancellationToken ct = default);
		Task<SitesDto> CreateAsync(SitesDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(int id, SitesDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(int id, CancellationToken ct = default);
	}
}

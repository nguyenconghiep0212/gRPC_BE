using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface IProject
	{
		Task<IEnumerable<ProjectResponse>> GetAllAsync(PaginationDto body,CancellationToken ct = default);
		Task<ProjectResponse?> GetAsync(int id, CancellationToken ct = default);
		Task<ProjectDto> CreateAsync(ProjectDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(int id, ProjectDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(int id, CancellationToken ct = default);
	}
}

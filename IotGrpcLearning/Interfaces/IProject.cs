using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IProject
{
    Task<ProjectDto> CreateAsync(ProjectDto dto, CancellationToken ct = default);
    Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ListDto<ProjectResponse>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, ProjectDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // Add project member operations to repository
    Task<List<ProjectMemberResponse>> GetProjectMembersAsync(int projectId, CancellationToken ct = default);
    Task<List<ProjectMemberDto>> AddProjectMembersAsync(int projectId, int[] employeeIds, CancellationToken ct = default);
}

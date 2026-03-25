using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

public interface IProject
{
    Task<ProjectDto> CreateAsync(ProjectDto dto, CancellationToken ct = default);
    Task<ProjectResponse?> GetAsync(int id, CancellationToken ct = default);
    Task<ListDto<ProjectResponse>> GetAllAsync(PaginationDto body, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, ProjectDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    
    // Ensure these are in the interface
    Task<List<ProjectMemberResponse>> GetProjectMembers(int projectId, CancellationToken ct);
    Task<List<ProjectMemberDto>> AddMembersToProject(int projectId, int[] employeeIds, CancellationToken ct);
}

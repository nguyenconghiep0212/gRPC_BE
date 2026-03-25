using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;

namespace IotGrpcLearning.Services;

/// <summary>
/// Service layer for Project operations.
/// Delegates all data access to IProjectRepository (follows SOLID principles).
/// </summary>
public sealed class ProjectService : IProject
{
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<ProjectDto> CreateAsync(ProjectDto dto, CancellationToken ct = default)
        => _repository.CreateAsync(dto, ct);

    public Task<ProjectResponse?> GetAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<ListDto<ProjectResponse>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        => _repository.GetAllAsync(body, ct);

    public Task<bool> UpdateAsync(int id, ProjectDto dto, CancellationToken ct = default)
        => _repository.UpdateAsync(id, dto, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);

    // ✅ FIXED: No more direct instantiation, no SQL in service
    public Task<List<ProjectMemberResponse>> GetProjectMembers(int projectId, CancellationToken ct)
        => _repository.GetProjectMembersAsync(projectId, ct);

    public Task<List<ProjectMemberDto>> AddMembersToProject(int projectId, int[] employeeIds, CancellationToken ct)
        => _repository.AddProjectMembersAsync(projectId, employeeIds, ct);
}  

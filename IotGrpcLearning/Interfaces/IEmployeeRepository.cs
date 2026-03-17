using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces;

/// <summary>
/// Repository for Employee data access with optimized queries.
/// </summary>
public interface IEmployeeRepository
{
    Task<EmployeesDto> CreateAsync(EmployeesDto dto, CancellationToken ct = default);
    Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ListDto<EmployeeResponse>> GetAllAsync(PaginationDto pagination, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, EmployeesDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
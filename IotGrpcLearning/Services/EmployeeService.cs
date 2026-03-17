using Grpc.Core;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Data.Sqlite;
using System.Numerics;
using System.Xml.Linq;

namespace IotGrpcLearning.Services;

public sealed class EmployeeService : IEmployee
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<EmployeesDto> CreateAsync(EmployeesDto dto, CancellationToken ct = default)
         => _repository.CreateAsync(dto, ct);

    public Task<EmployeeResponse?> GetAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<ListDto<EmployeeResponse>> GetAllAsync(PaginationDto body, CancellationToken ct = default)
        => _repository.GetAllAsync(body, ct);

    public Task<bool> UpdateAsync(int id, EmployeesDto dto, CancellationToken ct = default)
        => _repository.UpdateAsync(id, dto, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}

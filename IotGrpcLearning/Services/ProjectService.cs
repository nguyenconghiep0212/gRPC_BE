using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Proto;
using Microsoft.Data.Sqlite;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Linq;

namespace IotGrpcLearning.Services
{
    public sealed class ProjectService : IProject
    {
        private readonly ISqliteConnectionFactory _dbFactory;
        private readonly IProjectRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        public ProjectService(ISqliteConnectionFactory dbFactory, IProjectRepository repository, IEmployeeRepository employeeRepository)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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



        public async Task<List<ProjectMemberResponse>> GetProjectMembers(int projectId, CancellationToken ct)
        {
            EmployeeService _employeeService = new EmployeeService(_employeeRepository);
            var list = new List<ProjectMemberResponse>();

            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT pe.id, pe.employee_id " +
                "FROM ProjectEmployee pe " +
                "WHERE pe.project_id = @projectId;";

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync(ct))
            {
                var id = reader.GetInt32(0);
                var employeeId = reader.GetInt32(0);

                EmployeeResponse employeeDetail = await _employeeService.GetAsync(employeeId, ct);
                ProjectResponse projectDetail = await GetAsync(projectId, ct);
                list.Add(new ProjectMemberResponse(id, projectDetail, employeeDetail));
            }

            return list;
        }

        public async Task<List<ProjectMemberDto>> AddMembersToProject(int projectId, int[] employeeIds, CancellationToken ct)
        {
            var list = new List<ProjectMemberDto>();

            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO ProjectEmployee (project_id, employee_id) " +
                "VALUES (@project_id, @employee_id); " +
                "SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@project_id", projectId);

            foreach (var employeeId in employeeIds)
            {
                cmd.Parameters.AddWithValue("@employee_id", employeeId);

                var result = await cmd.ExecuteScalarAsync(ct);
                var newId = Convert.ToInt32(result);
                list.Add(new ProjectMemberDto(newId, projectId, employeeId));
            }

            return list;
        }
    }
}

namespace IotGrpcLearning.Models;

public record ProjectMemberDto(
	int Id,
	int project_id,
	int employee_id
	);

public record ProjectMemberResponse(
	int Id,
	ProjectResponse project,
	EmployeeResponse employee
	);


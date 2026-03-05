namespace IotGrpcLearning.Models;

public record TestSuiteDto(
	int Id,
	string Name,
	int MachineId,
	string Path,
	string Detail
	);



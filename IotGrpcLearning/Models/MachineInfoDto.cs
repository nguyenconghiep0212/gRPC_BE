namespace IotGrpcLearning.Models;

public record MachineInfoDto(
	int Id,
	int MachineId,
	int Project,
	int LineOverseer,
	int TestSuite
	);
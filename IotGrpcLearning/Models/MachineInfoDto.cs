namespace IotGrpcLearning.Models;

public record MachineInfoDto(
	int Id,
	int Project,
	int LineOverseer,
	int TestSuite
	);
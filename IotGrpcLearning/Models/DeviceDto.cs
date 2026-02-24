namespace IotGrpcLearning.Models;

public record DeviceDto(
	string DeviceId,
	string Health,
	string? Details,
	bool Connected,
	long LastSeenUnixMs,
	Dictionary<string, string> Tags);
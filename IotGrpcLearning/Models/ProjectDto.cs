namespace IotGrpcLearning.Models;

public record ProjectDto(
	int Id,
	string Name,
	int CustomerId,
	int SiteId,
	string Details
	);

public record ProjectResponse(
	int Id,
	string Name,
	int CustomerId,
	string Customer,
	int SiteId,
	string Site,
	string Details
	);


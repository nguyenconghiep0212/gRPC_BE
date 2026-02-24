using System.ComponentModel.DataAnnotations;

namespace IotGrpcLearning.Models;

public class CommandRequestDto
{
	[Required]
	public string Name { get; set; } = string.Empty;

	public Dictionary<string, string>? Args { get; set; }
}
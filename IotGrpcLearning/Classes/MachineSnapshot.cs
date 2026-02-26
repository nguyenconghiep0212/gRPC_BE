namespace IotGrpcLearning.Classes
{
	public sealed class MachineSnapshot
	{
		public required string DeviceId { get; init; }
		public string Health { get; set; } = "UNKNOWN";
		public string? Details { get; set; }
		public long LastSeenUnixMs { get; set; }
		public bool Connected { get; set; }
		public Dictionary<string, string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	}
}

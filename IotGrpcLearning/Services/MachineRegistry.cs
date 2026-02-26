using IotGrpcLearning.Classes;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Proto;
using System.Collections.Concurrent;

namespace IotGrpcLearning.Services
{
    public class MachineRegistry: IMachineRegistry
    {
		private readonly ConcurrentDictionary<string, MachineSnapshot> _machines = new(StringComparer.OrdinalIgnoreCase);
		private readonly ISqliteConnectionFactory _dbFactory;
		public MachineRegistry(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}


		public void InitMachine(string deviceId)
		{
			var snap = _machines.GetOrAdd(deviceId, id => new MachineSnapshot
			{
				DeviceId = id,
				LastSeenUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			});
			snap.Connected = false;
		}

		public void MarkConnected(string deviceId)
		{
			var snap = _machines.GetOrAdd(deviceId, id => new MachineSnapshot
			{
				DeviceId = id,
				LastSeenUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			});

			snap.Connected = true;
			snap.LastSeenUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public void MarkDisconnected(string deviceId)
		{
			if (_machines.TryGetValue(deviceId, out var snap))
			{
				snap.Connected = false;
				snap.LastSeenUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			}
		}

		public void UpdateStatus(DeviceStatusResponse status)
		{
			var id = (status.DeviceId ?? string.Empty).Trim();
			if (string.IsNullOrWhiteSpace(id)) return;

			var snap = _machines.GetOrAdd(id, _ => new MachineSnapshot { DeviceId = id });

			snap.Health = string.IsNullOrWhiteSpace(status.Health) ? "UNKNOWN" : status.Health!;
			snap.Details = status.Details;
			snap.LastSeenUnixMs = status.UnixMs > 0 ? status.UnixMs : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			snap.Connected = true;
		}

		public IReadOnlyCollection<MachineSnapshot> GetAll() => _machines.Values.ToArray();

		public MachineSnapshot? Get(string deviceId)
			=> _machines.TryGetValue(deviceId, out var snap) ? snap : null; 
	}
}

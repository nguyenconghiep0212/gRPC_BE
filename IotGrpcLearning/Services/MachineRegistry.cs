using IotGrpcLearning.Classes;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Proto;
using System.Collections.Concurrent;

namespace IotGrpcLearning.Services
{
    public class MachineRegistry: IMachineRegistry
    {
		private readonly ConcurrentDictionary<int, MachineHealth> _machines = new();
		private readonly ISqliteConnectionFactory _dbFactory;
		public MachineRegistry(ISqliteConnectionFactory dbFactory)
		{
			_dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
		}

        public void InitMachine(int deviceId)
        {
            throw new NotImplementedException();
        }

        public void MarkConnected(int deviceId)
        {
            throw new NotImplementedException();
        }

        public void MarkDisconnected(int deviceId)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatus(DeviceStatusResponse status)
        {
            throw new NotImplementedException();
        }
    }
}

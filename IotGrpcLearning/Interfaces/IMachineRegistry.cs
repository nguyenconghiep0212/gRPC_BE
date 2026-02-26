using System.Collections.Concurrent;
using IotGrpcLearning.Proto;
using System.Data.Common;
using IotGrpcLearning.Classes;
using IotGrpcLearning.Services;

namespace IotGrpcLearning.Interfaces
{
	public interface IMachineRegistry
	{
		void InitMachine(string deviceId);
		void MarkConnected(string deviceId);
		void MarkDisconnected(string deviceId);
		void UpdateStatus(DeviceStatusResponse status);
		IReadOnlyCollection<MachineSnapshot> GetAll();
		MachineSnapshot? Get(string deviceId);
 	}  
}

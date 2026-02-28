using System.Collections.Concurrent;
using IotGrpcLearning.Proto;
using System.Data.Common;
using IotGrpcLearning.Classes;
using IotGrpcLearning.Services;

namespace IotGrpcLearning.Interfaces
{
	public interface IMachineRegistry
	{
		void InitMachine(int deviceId);
		void MarkConnected(int deviceId);
		void MarkDisconnected(int deviceId);
		void UpdateStatus(DeviceStatusResponse status); 
 	}  
}

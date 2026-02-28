using Grpc.Core;
using IotGrpcLearning.Proto;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace IotGrpcLearning.Services;

public interface ICommandBus
{
	ChannelReader<Command> Subscribe(int machineId);
	ValueTask EnqueueCommandAsync(int machineId, Command command, CancellationToken ct = default);
}

public sealed class InMemoryCommandBus : ICommandBus
{
	private readonly ConcurrentDictionary<int, Channel<Command>> _channels = new();

	public ChannelReader<Command> Subscribe(int machineId)
	{
		Channel<Command> channel = _channels.GetOrAdd(machineId, _ => Channel.CreateUnbounded<Command>());
		return channel.Reader;
	}

	public async ValueTask EnqueueCommandAsync(int machineId, Command command, CancellationToken ct = default)
	{
		// Get or create the channel for the machine
		Channel<Command> channel = _channels.GetOrAdd(machineId, _ => Channel.CreateUnbounded<Command>());  
		// Write the CommandResponse to the channel
		await channel.Writer.WriteAsync(command, ct);
	}

	public ConcurrentDictionary<int, Channel<Command>> GetChannelsDictionary()
	{
		return _channels;
	}
}

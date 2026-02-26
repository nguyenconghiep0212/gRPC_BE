using Microsoft.AspNetCore.Mvc;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using IotGrpcLearning.Proto;
using IotGrpcLearning.Interfaces;

namespace IotGrpcLearning.Controllers;

[ApiController]
[Route("api/devicesRegistry")]
public class MachinesRegistryController : ControllerBase
{
	private readonly IMachineService _service;
	private readonly IMachineRegistry _registry;
	private readonly ICommandBus _bus;

	public MachinesRegistryController(IMachineService service, IMachineRegistry registry, ICommandBus bus)
	{
		_service = service;
		_registry = registry;
		_bus = bus;
	} 

 	// POST /api/devices/{deviceId}/commands
	[HttpPost("{deviceId}/commands")]
	public async Task<IActionResult> SendCommand(
		string deviceId,
		[FromBody] CommandRequestDto request,
		CancellationToken ct)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);

		// Optional: validate device exists in registry
		// If you prefer allowing enqueue even when offline, remove this block.
		var d = _service.GetAsync(deviceId);
		if (d is null)
			return NotFound(new { error = "device not found" });

		var cmd = new Command
		{
			CommandId = Guid.NewGuid().ToString("N"),
			Name = request.Name.Trim()
		};

		if (request.Args is not null)
		{
			foreach (var (k, v) in request.Args)
			{
				if (!string.IsNullOrWhiteSpace(k))
					cmd.Args[k] = v ?? string.Empty;
			}
		}

		await _bus.EnqueueCommandAsync(deviceId, cmd, ct);

		return Ok(new
		{
			queued = true,
			deviceId,
			command = new { cmd.CommandId, cmd.Name, args = cmd.Args }
		});
	} 

}
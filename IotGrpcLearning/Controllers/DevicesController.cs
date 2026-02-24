using Microsoft.AspNetCore.Mvc;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using IotGrpcLearning.Proto;

namespace IotGrpcLearning.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
	private readonly IDeviceRegistry _registry;
	private readonly ICommandBus _bus;

	public DevicesController(IDeviceRegistry registry, ICommandBus bus)
	{
		_registry = registry;
		_bus = bus;
	}

	// GET /api/devices/list
	[HttpGet("list")]
	public ActionResult<IEnumerable<DeviceDto>> List()
	{
		var devices = _registry.GetAll()
			.OrderBy(d => d.DeviceId)
			.Select(d => new DeviceDto(
				d.DeviceId,
				d.Health,
				d.Details,
				d.Connected,
				d.LastSeenUnixMs,
				d.Tags));

		return Ok(devices);
	}

	// GET /api/devices/{deviceId}/detail
	[HttpGet("{deviceId}/detail")]
	public ActionResult<DeviceDto> Detail(string deviceId)
	{
		var d = _registry.Get(deviceId);
		if (d is null)
			return NotFound(new { error = "device not found" });

		return Ok(new DeviceDto(
			d.DeviceId,
			d.Health,
			d.Details,
			d.Connected,
			d.LastSeenUnixMs,
			d.Tags));
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
		var d = _registry.Get(deviceId);
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
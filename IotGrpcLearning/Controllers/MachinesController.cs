using Microsoft.AspNetCore.Mvc;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using IotGrpcLearning.Proto;
using IotGrpcLearning.Interfaces;

namespace IotGrpcLearning.Controllers;

[ApiController]
[Route("api/devices")]
public class MachinesController : ControllerBase
{
	private readonly IMachineService _service;

	public MachinesController(IMachineService service)
	{
		_service = service;
	}

	// GET /api/devices/list
	[HttpGet("list")]
	public async Task<ActionResult<IEnumerable<MachineDto>>> List()
	{
		var devices = await _service.GetAllAsync();
		var result = devices.Select(d => new MachineDto(
			d.Id,
			d.MachineInfoId,
			d.Name,
			d.Details,
			d.Vendor,
			d.PurchasePrice,
			d.PurchaseDate,
			d.Status,
			d.Site));

		return Ok(devices);
	}

	// GET /api/devices/{deviceId}/detail
	[HttpGet("{deviceId}/detail")]
	public async Task<ActionResult<MachineDto>> Detail(string deviceId)
	{
		var d = await _service.GetAsync(deviceId);
		if (d is null)
			return NotFound(new { error = "device not found" });

		return Ok(new MachineDto(d.Id,
			d.MachineInfoId,
			d.Name,
			d.Details,
			d.Vendor,
			d.PurchasePrice,
			d.PurchaseDate,
			d.Status,
			d.Site));
	}

	// POST /api/devices/create
	[HttpPost("create")]
	public async Task<MachineDto> Create(MachineDto dto)
	{
		var d = new MachineDto(
			dto.Id,
			dto.MachineInfoId,
			dto.Name,
			dto.Details,
			dto.Vendor,
			dto.PurchasePrice,
			dto.PurchaseDate,
			dto.Status,
			dto.Site);
		var created = await _service.CreateAsync(d);
		return new MachineDto(created.Id, created.MachineInfoId, created.Name, created.Details, created.Vendor, created.PurchasePrice, created.PurchaseDate, created.Status, created.Site);
	}

}
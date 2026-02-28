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

	// POST /api/machine/list
	[HttpPost("list")]
	public async Task<ActionResult<IEnumerable<MachineDto>>> List(PaginationDto body)
	{
		var devices = await _service.GetAllAsync(body);
		var result = devices.Select(d => new MachineDto(
			d.Id,
			d.Name,
			d.Alias,
			d.Details,
			d.Vendor,
			d.PurchasePrice,
			d.PurchaseDate,
			d.Site));

		return Ok(devices);
	}

	// GET /api/machine/{machineId}/detail
	[HttpGet("{machineId}/detail")]
	public async Task<ActionResult<MachineDto>> Detail(int machineId)
	{
		var d = await _service.GetAsync(machineId);
		if (d is null)
			return NotFound(new { error = "machine not found" });

		return Ok(new MachineDto(d.Id,
 			d.Name,
			d.Alias,
			d.Details,
			d.Vendor,
			d.PurchasePrice,
			d.PurchaseDate,
 			d.Site));
	}

	// POST /api/machine/create
	[HttpPost("create")]
	public async Task<MachineDto> Create(MachineDto dto)
	{
		var d = new MachineDto(
			dto.Id,
			dto.Name,
			dto.Alias,
			dto.Details,
			dto.Vendor,
			dto.PurchasePrice,
			dto.PurchaseDate,
			dto.Site);
		var created = await _service.CreateAsync(d);
		return new MachineDto(created.Id, created.Name, created.Alias, created.Details, created.Vendor, created.PurchasePrice, created.PurchaseDate, created.Site);
	}

}
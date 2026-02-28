using Microsoft.AspNetCore.Mvc;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using IotGrpcLearning.Proto;
using IotGrpcLearning.Interfaces;

namespace IotGrpcLearning.Controllers;

[ApiController]
[Route("api/machine_status")]
public class MachineStatusController : ControllerBase
{
	private readonly IMachineStatusService _service;

	public MachineStatusController(IMachineStatusService service)
	{
		_service = service;
	}

	// POST /api/machine_status/list
	[HttpPost("list")]
	public async Task<ActionResult<IEnumerable<MachineStatusDto>>> List(PaginationDto body)
	{
		var devices = await _service.GetAllAsync(body);
		var result = devices.Select(d => new MachineStatusDto(
			d.Id,
			d.MachineId,
			d.Health,
			d.IsOnline,
			d.LastOnline
			));

		return Ok(devices);
	}

	// GET /api/machine_status/{machineStatusId}/detail
	[HttpGet("{machineStatusId}/detail")]
	public async Task<ActionResult<MachineStatusDto>> Detail(int machineStatusId)
	{
		var d = await _service.GetAsync(machineStatusId);
		if (d is null)
			return NotFound(new { error = "machine not found" });

		return Ok(new MachineStatusDto(d.Id,
			d.MachineId,
			d.Health,
			d.IsOnline,
			d.LastOnline));
	}

	// POST /api/machine_status/create
	[HttpPost("create")]
	public async Task<MachineStatusDto> Create(MachineStatusDto dto)
	{
		var d = new MachineStatusDto(
			dto.Id,
			dto.MachineId,
			dto.Health,
			dto.IsOnline,
			dto.LastOnline);
		var created = await _service.CreateAsync(d);
		return new MachineStatusDto(created.Id, created.MachineId, created.Health, created.IsOnline, created.LastOnline);
	}

	// PUT /api/machine_status/{machineStatusId}/update
	[HttpPut("{machineStatusId}/update")]
	public async Task<ActionResult<bool>> Update(int machineStatusId, MachineStatusDto dto)
	{
		var d = new MachineStatusDto(
			dto.Id,
			dto.MachineId,
			dto.Health,
			dto.IsOnline,
			dto.LastOnline
			);
		var updated = await _service.UpdateAsync(machineStatusId, d);
		if (!updated)
			return NotFound(new { error = "machine status not found" });
		return Ok(new { message = "machine status updated successfully" });
	}

	// DELETE /api/machine_status/{machineStatusId}/delete
	[HttpDelete("{machineStatusId}/delete")]
	public async Task<ActionResult<bool>> Delete(int machineStatusId)
	{
		var deleted = await _service.DeleteAsync(machineStatusId);
		if (!deleted)
			return NotFound(new { error = "machine status not found" });
		return Ok(new { message = "machine status deleted successfully" });
	}
}
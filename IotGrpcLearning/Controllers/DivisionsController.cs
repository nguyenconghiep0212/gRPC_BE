using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/divisions")]
	public class DivisionsController : ControllerBase
	{

		private readonly IDivision _service;

		public DivisionsController(IDivision service)
		{
			_service = service;
		}

		// POST /api/divions/create
		[HttpPost("create")]
		public async Task<DivisionsDto> Create(DivisionsDto dto)
		{
			var d = new DivisionsDto(
				dto.Id,
				dto.Name
				);
			var created = await _service.CreateAsync(d);
			return new DivisionsDto(created.Id, created.Name);
		}

		// POST /api/divisions/list
		[HttpPost("list")]
		public async Task<ActionResult<IEnumerable<DivisionsDto>>> List(PaginationDto body)
		{
			var divisions = await _service.GetAllAsync(body);
			var result = divisions.Select(d => new DivisionsDto(
				d.Id,
				d.Name
				));
			return Ok(divisions);
		}

		// GET /api/divisions/{divisionsId}/detail
		[HttpGet("{divisionsId}/detail")]
		public async Task<ActionResult<IEnumerable<DivisionsDto>>> Detail(int divisionsId)
		{
			var division = await _service.GetAsync(divisionsId);
			return Ok(division);
		}

		// PUT /api/divisions/{divisionsId}/update
		[HttpPut("{divisionsId}/update")]
		public async Task<ActionResult<bool>> Update(int divisionsId, DivisionsDto dto)
		{
			var d = new DivisionsDto(
				dto.Id,
				dto.Name
				);
			var updated = await _service.UpdateAsync(divisionsId, d);
			if (!updated)
				return NotFound(new { error = "divisions not found" });
			return Ok(new { message = "divisions updated successfully" });
		}

		// DELETE /api/divisions/{divisionsId}/delete
		[HttpDelete("{divisionsId}/delete")]
		public async Task<ActionResult<bool>> Delete(int divisionsId)
		{
			var deleted = await _service.DeleteAsync(divisionsId);
			if (!deleted)
				return NotFound(new { error = "divisions not found" });
			return Ok(new { message = "divisions deleted successfully" });
		}
	}
}

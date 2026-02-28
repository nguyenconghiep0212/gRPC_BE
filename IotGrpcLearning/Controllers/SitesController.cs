using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/sites")]
	public class SitesController : ControllerBase
	{

		private readonly ISite _service;

		public SitesController(ISite service)
		{
			_service = service;
		}

		// POST /api/sites/create
		[HttpPost("create")]
		public async Task<SitesDto> Create(SitesDto dto)
		{
			var d = new SitesDto(
				dto.Id,
				dto.Name,
				dto.Location,
				dto.Address
				);
			var created = await _service.CreateAsync(d);
			return new SitesDto(created.Id, created.Name, created.Location, created.Address);
		}

		// POST /api/sites/list
		[HttpPost("list")]
		public async Task<ActionResult<IEnumerable<SitesDto>>> List(PaginationDto body)
		{
			var sites = await _service.GetAllAsync(body);
			var result = sites.Select(d => new SitesDto(
				d.Id,
				d.Name,
				d.Location,
				d.Address
				));
			return Ok(sites);
		}

		// GET /api/sites/{sitesId}/detail
		[HttpGet("{sitesId}/detail")]
		public async Task<ActionResult<IEnumerable<SitesDto>>> Detail(int sitesId)
		{
			var role = await _service.GetAsync(sitesId);
			return Ok(role);
		}

		// PUT /api/sites/{sitesId}/update
		[HttpPut("{sitesId}/update")]
		public async Task<ActionResult<bool>> Update(int sitesId, SitesDto dto)
		{
			var d = new SitesDto(
				dto.Id,
				dto.Name,
				dto.Location,
				dto.Address
				);
			var updated = await _service.UpdateAsync(sitesId, d);
			if (!updated)
				return NotFound(new { error = "sites not found" });
			return Ok(new { message = "sites updated successfully" });
		}

		// DELETE /api/sites/{sitesId}/delete
		[HttpDelete("{sitesId}/delete")]
		public async Task<ActionResult<bool>> Delete(int sitesId)
		{
			var deleted = await _service.DeleteAsync(sitesId);
			if (!deleted)
				return NotFound(new { error = "sites not found" });
			return Ok(new { message = "sites deleted successfully" });
		}
	}
}

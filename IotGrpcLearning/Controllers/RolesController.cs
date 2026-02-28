using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/roles")]
	public class RolesController : ControllerBase
	{

		private readonly IRole _service;

		public RolesController(IRole service)
		{
			_service = service;
		}

		// POST /api/roles/create
		[HttpPost("create")]
		public async Task<RolesDto> Create(RolesDto dto)
		{
			var d = new RolesDto(
				dto.Id,
				dto.Name
				);
			var created = await _service.CreateAsync(d);
			return new RolesDto(created.Id, created.Name);
		}

		// POST /api/roles/list
		[HttpPost("list")]
		public async Task<ActionResult<IEnumerable<RolesDto>>> List(PaginationDto body)
		{
			var roles = await _service.GetAllAsync(body);
			var result = roles.Select(d => new RolesDto(
				d.Id,
				d.Name
				));
			return Ok(roles);
		}

		// GET /api/roles/{rolesId}/detail
		[HttpGet("{rolesId}/detail")]
		public async Task<ActionResult<IEnumerable<RolesDto>>> Detail(int rolesId)
		{
			var role = await _service.GetAsync(rolesId);
			return Ok(role);
		}

		// PUT /api/roles/{rolesId}/update
		[HttpPut("{rolesId}/update")]
		public async Task<ActionResult<bool>> Update(int rolesId, RolesDto dto)
		{
			var d = new RolesDto(
				dto.Id,
				dto.Name
				);
			var updated = await _service.UpdateAsync(rolesId, d);
			if (!updated)
				return NotFound(new { error = "roles not found" });
			return Ok(new { message = "roles updated successfully" });
		}

		// DELETE /api/roles/{rolesId}/delete
		[HttpDelete("{rolesId}/delete")]
		public async Task<ActionResult<bool>> Delete(int rolesId)
		{
			var deleted = await _service.DeleteAsync(rolesId);
			if (!deleted)
				return NotFound(new { error = "roles not found" });
			return Ok(new { message = "roles deleted successfully" });
		}
	}
}

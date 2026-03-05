using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectController : ControllerBase
	{

		private readonly IProject _service;

		public ProjectController(IProject service)
		{
			_service = service;
		}

		// POST /api/projects/create
		[HttpPost("create")]
		public async Task<ProjectDto> Create(ProjectDto dto)
		{
			var d = new ProjectDto(
				dto.Id,
				dto.Name,
				dto.CustomerId,
				dto.SiteId,
				dto.Details
				);
			var created = await _service.CreateAsync(d);
			return new ProjectDto(created.Id, created.Name, created.CustomerId, created.SiteId, created.Details);
		}

		// POST /api/projects/list
		[HttpPost("list")]
		public async Task<ActionResult<IEnumerable<ProjectResponse>>> List(PaginationDto body)
		{
			var sites = await _service.GetAllAsync(body);
			var result = sites.Select(d => new ProjectResponse(
				d.Id,
				d.Name,
				d.CustomerId,
				d.Customer,
				d.SiteId,
				d.Site,
				d.Details
				));
			return Ok(sites);
		}

		// GET /api/projects/{projectId}/detail
		[HttpGet("{projectId}/detail")]
		public async Task<ActionResult<IEnumerable<ProjectResponse>>> Detail(int projectId)
		{
			var role = await _service.GetAsync(projectId);
			return Ok(role);
		}

		// PUT /api/projects/{projectId}/update
		[HttpPut("{projectId}/update")]
		public async Task<ActionResult<bool>> Update(int projectId, ProjectDto dto)
		{
			var d = new ProjectDto(
				dto.Id,
				dto.Name,
				dto.CustomerId,
				dto.SiteId,
				dto.Details
				);
			var updated = await _service.UpdateAsync(projectId, d);
			if (!updated)
				return NotFound(new { error = "project not found" });
			return Ok(new { message = "project updated successfully" });
		}

		// DELETE /api/projects/{projectId}/delete
		[HttpDelete("{projectId}/delete")]
		public async Task<ActionResult<bool>> Delete(int projectId)
		{
			var deleted = await _service.DeleteAsync(projectId);
			if (!deleted)
				return NotFound(new { error = "project not found" });
			return Ok(new { message = "project deleted successfully" });
		}
	}
}

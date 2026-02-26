using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/vendors")]
	public class VendorController : ControllerBase
	{

		private readonly IVendor _service;

		public VendorController(IVendor service)
		{
			_service = service;
		}

		// POST /api/vendors/create
		[HttpPost("create")]
		public async Task<VendorDto> Create(VendorDto dto)
		{
			var d = new VendorDto(
				dto.Id,
				dto.Name
				);
			var created = await _service.CreateAsync(d);
			return new VendorDto(created.Id, created.Name);
		}

		// GET /api/vendors/list
		[HttpGet("list")]
		public async Task<ActionResult<IEnumerable<VendorDto>>> List()
		{
			var devices = await _service.GetAllAsync();
			var result = devices.Select(d => new VendorDto(
				d.Id,
				d.Name
				));
			return Ok(devices);
		}

	}
}

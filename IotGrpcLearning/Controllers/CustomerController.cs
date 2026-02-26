using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/customers")]
	public class CustomerController : ControllerBase
	{

		private readonly ICustomer _service;

		public CustomerController(ICustomer service)
		{
			_service = service;
		}

		// POST /api/customers/create
		[HttpPost("create")]
		public async Task<CustomerDto> Create(CustomerDto dto)
		{
			var d = new CustomerDto(
				dto.Id,
				dto.Name
				);
			var created = await _service.CreateAsync(d);
			return new CustomerDto(created.Id, created.Name);
		}

		// GET /api/customers/list
		[HttpGet("list")]
		public async Task<ActionResult<IEnumerable<CustomerDto>>> List()
		{
			var devices = await _service.GetAllAsync();
			var result = devices.Select(d => new CustomerDto(
				d.Id,
				d.Name
				));
			return Ok(devices);
		}

	}
}

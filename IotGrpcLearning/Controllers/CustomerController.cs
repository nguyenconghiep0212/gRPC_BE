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

		// POST /api/customers/list
		[HttpPost("list")]
		public async Task<ActionResult<IEnumerable<CustomerDto>>> List(PaginationDto body)
		{
			var customer = await _service.GetAllAsync(body);
			var result = customer.Select(d => new CustomerDto(
				d.Id,
				d.Name
				));
			return Ok(customer);
		}

		// GET /api/customers/{customerId}/detail
		[HttpGet("{customerId}/detail")]
		public async Task<ActionResult<IEnumerable<CustomerDto>>> Detail(int customerId)
		{
			var customer = await _service.GetAsync(customerId);
			return Ok(customer);
		}

		// PUT /api/customers/{customerId}/update
		[HttpPut("{customerId}/update")]
		public async Task<ActionResult<bool>> Update(int customerId, CustomerDto dto)
		{
			var d = new CustomerDto(
				dto.Id,
				dto.Name
				);
			var updated = await _service.UpdateAsync(customerId, d);
			if (!updated)
				return NotFound(new { error = "customer not found" });
			return Ok(new { message = "customer updated successfully" });
		}

		// DELETE /api/customer/{customerId}/delete
		[HttpDelete("{customerId}/delete")]
		public async Task<ActionResult<bool>> Delete(int customerId)
		{
			var deleted = await _service.DeleteAsync(customerId);
			if (!deleted)
				return NotFound(new { error = "customer not found" });
			return Ok(new { message = "customer deleted successfully" });
		}
	}
}

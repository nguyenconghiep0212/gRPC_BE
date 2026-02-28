using Google.Protobuf;
using IotGrpcLearning.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IotGrpcLearning.Controllers
{
	[ApiController]
	[Route("api/seed")]
	public sealed class SeedController : ControllerBase
	{
		private readonly Seeder _seeder;
		private readonly ILogger<SeedController> _logger;

		public SeedController(Seeder vendorSeeder, ILogger<SeedController> logger)
		{
			_seeder = vendorSeeder ?? throw new ArgumentNullException(nameof(vendorSeeder));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}


		/// <summary>
		/// DELETE /api/seed/remove_all
		/// </summary>
		[HttpDelete("remove_all")]
		public Task<IActionResult> RemoveAllTableData(CancellationToken ct)
		=> Seed(_seeder.ClearAllDataAsync, "Data cleared", ct);

		/// <summary>
		/// POST /api/seed/seed_all
		/// </summary>
		[HttpPost("seed_all")]
		public async Task<IActionResult> SeedAll(CancellationToken ct = default)
		{
			var steps = new (Func<CancellationToken, Task> Work, string Message)[]
				{
				(_seeder.SeedVendorAsync,    "Vendors seeded"),
				(_seeder.SeedCustomerAsync,  "Customers seeded"),
				(_seeder.SeedSiteAsync,      "Sites seeded"),
				(_seeder.SeedRolesAsync,     "Roles seeded"),
				(_seeder.SeedDivisionAsync,  "Divisions seeded"),
				(_seeder.SeedMachineAsync,   "Machines seeded")
				};
			foreach (var step in steps)
			{
				var result = await Seed(step.Work, step.Message, ct);
				if (result is not OkObjectResult)
					return result;
			}

			return Ok(new { message = "All seed operations completed" });

		}


		/// <summary>
		/// POST /api/seed/vendors
		/// </summary>
		[HttpPost("vendors")]
		public Task<IActionResult> SeedVendors(CancellationToken ct = default)
		   => Seed(_seeder.SeedVendorAsync, "Vendors seeded", ct);


		/// <summary>
		/// POST /api/seed/customer
		/// </summary>
		[HttpPost("customers")]
		public Task<IActionResult> SeedCustomers(CancellationToken ct = default)
		  => Seed(_seeder.SeedCustomerAsync, "Customers seeded", ct);

		/// <summary>
		/// POST /api/seed/sites
		/// </summary>
		[HttpPost("sites")]
		public Task<IActionResult> SeedSites(CancellationToken ct = default)
		  => Seed(_seeder.SeedSiteAsync, "Sites seeded", ct);

		/// <summary>
		/// POST /api/seed/roles
		/// </summary>
		[HttpPost("roles")]
		public Task<IActionResult> SeedRoles(CancellationToken ct = default)
		  => Seed(_seeder.SeedRolesAsync, "Sites seeded", ct);

		/// <summary>
		/// POST /api/seed/divisions
		/// </summary>
		[HttpPost("divisions")]
		public Task<IActionResult> SeedDivisions(CancellationToken ct = default)
		  => Seed(_seeder.SeedDivisionAsync, "Divisions seeded", ct);

		/// <summary>
		/// POST /api/seed/machines
		/// </summary>
		[HttpPost("machines")]
		public Task<IActionResult> SeedMachines(CancellationToken ct = default)
		  => Seed(_seeder.SeedMachineAsync, "Machines seeded", ct);

		private async Task<IActionResult> Seed(Func<CancellationToken, Task> work, string message, CancellationToken ct = default)
		{
			try
			{
				await work(ct);
				return Ok(new { message });
			}
			catch (OperationCanceledException)
			{
				return StatusCode(499, new { error = "Client closed request / canceled" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Seed operation failed");
				return StatusCode(500, new { error = "Seed operation failed", detail = ex.Message });
			}
		}
	}
}
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Services;

namespace IotGrpcLearning.Infrastructure
{
	public static class Singleton
	{
		public static void AddSingleton(this WebApplicationBuilder builder)
		{
			// Seeder singleton
			builder.Services.AddSingleton<Seeder>();

			// Services singleton
			builder.Services.AddSingleton<ICommandBus, InMemoryCommandBus>();
			builder.Services.AddSingleton<IMachineRegistry, MachineRegistry>();
			builder.Services.AddSingleton<IMachineService, MachineService>();
			builder.Services.AddSingleton<IMachineStatusService, MachineStatusService>();
			builder.Services.AddSingleton<IVendor, VendorService>();
			builder.Services.AddSingleton<ICustomer, CustomerService>();
			builder.Services.AddSingleton<IRole, RoleService>();
			builder.Services.AddSingleton<IDivision, DivisionService>();
			builder.Services.AddSingleton<ISite, SiteService>();
		}
	}
}

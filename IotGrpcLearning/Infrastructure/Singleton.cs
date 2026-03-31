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
            // Service
            builder.Services.AddSingleton<ISqlHelper, SqlHelper>();
            builder.Services.AddSingleton<IPasswordService, PasswordService>();
            builder.Services.AddSingleton<ICommandBus, InMemoryCommandBus>();
			builder.Services.AddSingleton<IMachineRegistry, MachineRegistry>();
            builder.Services.AddSingleton<ILookupCache, LookupCache>();

            // Independent Table
            builder.Services.AddSingleton<IVendor, VendorService>();
			builder.Services.AddSingleton<ICustomer, CustomerService>();
			builder.Services.AddSingleton<IRole, RoleService>();
			builder.Services.AddSingleton<IDivision, DivisionService>();
			builder.Services.AddSingleton<ISite, SiteService>();
			builder.Services.AddSingleton<ITestSuite, TestSuiteService>();
			// Dependent Table
			builder.Services.AddSingleton<IMachineStatus, MachineStatusService>();
			builder.Services.AddSingleton<IMachineInfoService, MachineInfoService>();
			builder.Services.AddSingleton<IMachine, MachineService>();
			builder.Services.AddSingleton<IEmployee, EmployeeService>();
			builder.Services.AddSingleton<IProject, ProjectService>();
            // Relation Table 

            // Auth
            builder.Services.AddSingleton<IUser, UserService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

        }
    }
}

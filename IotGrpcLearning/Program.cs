using IotGrpcLearning.Proto;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddGrpc();

			// REST API (MVC Controllers)
			builder.Services.AddControllers();

			// DI: registry + command bus
			builder.Services.AddSingleton<ICommandBus, InMemoryCommandBus>();
			builder.Services.AddSingleton<IDeviceRegistry, DeviceRegistry>();

			// Vue dev server default: http://localhost:5173
			builder.Services.AddCors(o =>
			{
				o.AddPolicy("ui", p => p
					.WithOrigins("http://localhost:5173")
					.AllowAnyHeader()
					.AllowAnyMethod());
			});

			// Swagger (optional but very “enterprise”)
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			app.UseCors("ui");

			// Swagger (optional)
			app.UseSwagger();
			app.UseSwaggerUI();

			// Map MVC controllers
			app.MapControllers();

			// Map gRPC services
			app.MapGrpcService<DeviceGatewayService>();

			app.MapGet("/", () => "Device Gateway running. REST: /api, gRPC: DeviceGateway");

			//// Feed Commands Helper
			//app.MapPost("/cmd/{deviceId}/{name}", async (string deviceId, string name, ICommandBus bus, HttpContext http) =>
			//{
			//	Console.WriteLine($"Received command for device '{deviceId}' - '{name}'");
			//	var cmd = new Command
			//	{
			//		CommandId = Guid.NewGuid().ToString("N"),
			//		Name = name
			//	};

			//	if (cmd.Name == "SetThreshold")
			//	{
			//		// collect query string as args, e.g., ?key=value
			//		foreach (var (k, v) in http.Request.Query)
			//		{
			//			if (!string.IsNullOrWhiteSpace(k) && v.Count > 0)
			//				cmd.Args[k] = v[0]!;
			//		}
			//		await bus.EnqueueCommandAsync(deviceId, cmd, http.RequestAborted);
			//		return Results.Ok(new { queued = true, deviceId, cmd = new { cmd.CommandId, cmd.Name, Args = cmd.Args } });
			//	}
			//	if (cmd.Name == "StartHeartbeat")
			//	{
			//		if (http.Request.Query.Count == 0)
			//		{
			//			return Results.BadRequest(new { Error = "No parameter!", Query = new { interval = "int" } });
			//		}
			//		foreach (var (k, v) in http.Request.Query)
			//		{
			//			if (k != "interval")
			//			{
			//				return Results.BadRequest(new { Error = "Incorrect parameter!", Query = new { interval = "int" } });
			//			}
			//			if (!string.IsNullOrWhiteSpace(k) && v.Count > 0)
			//			{
			//				cmd.Args[k] = v[0]!;
			//			}
			//		}
			//		await bus.EnqueueCommandAsync(deviceId, cmd, http.RequestAborted);
			//		return Results.Ok(new { queued = true, deviceId, cmd = new { cmd.CommandId, cmd.Name, Args = cmd.Args } });
			//	}
			//	if (cmd.Name == "StopHeartbeat")
			//	{
			//		await bus.EnqueueCommandAsync(deviceId, cmd, http.RequestAborted);
			//		return Results.Ok(new { queued = true, deviceId, cmd = new { cmd.CommandId, cmd.Name, Args = cmd.Args } });
			//	}
			//	return Results.BadRequest(new { Error = "Incorrect command!" });
			//});
			////curl - X POST "https://localhost:7096/cmd/device-1/SetThreshold?metric=temperature&value=38.0" 

			app.Run();
		}
	}
}

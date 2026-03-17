using IotGrpcLearning.GrpcServices;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// REST API (MVC Controllers)
builder.Services.AddControllers();
builder.Services.AddCors(o =>
{
    o.AddPolicy("ui", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validate and register Sqlite connection factory
builder.AddValidatedSqlite();

// Health checks
builder.Services.AddHealthChecks();

// Configure PasswordOptions from configuration
builder.Services.Configure<PasswordOptions>(builder.Configuration.GetSection("Password"));

// Register infrastructure services (Phase 2)
builder.Services.AddSingleton<ISqlHelper, SqlHelper>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();

// Register lookup cache (Phase 3.4)
builder.Services.AddSingleton<ILookupCache, LookupCache>();

// Register repositories (Phase 3)
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IMachineRepository, MachineRepository>();

// Build the app
var app = builder.Build();

// Warm up the cache on startup
using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<ILookupCache>();
    await cache.RefreshAsync();
}

// Add middleware for cross-cutting concerns
app.UseCors("ui");

// Global exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Health endpoint
app.MapHealthChecks("/health");

// Map MVC controllers
app.MapControllers();

// Map gRPC services
app.MapGrpcService<MachineGatewayService>();

app.MapGet("/", () => "Device Gateway running. REST: /api, gRPC: DeviceGateway");

app.Run();

using IotGrpcLearning.GrpcServices;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Proto;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

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

// Validate and register Sqlite connection factory (extension handles validation and registration)
builder.AddValidatedSqlite();

// Health checks
builder.Services.AddHealthChecks();

// Configure PasswordOptions from configuration section "Password"
builder.Services.Configure<PasswordOptions>(builder.Configuration.GetSection("Password"));

// Register the exception middleware's dependencies (ILogger is registered by the host by default).
// Any additional infra services you add in Phase 1 should be registered here.

// Build the app
var app = builder.Build();

// Add middleware for cross-cutting concerns
app.UseCors("ui");

// Global exception handling middleware (minimal)
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

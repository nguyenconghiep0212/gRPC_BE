using IotGrpcLearning.GrpcServices;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Repositories;
using IotGrpcLearning.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

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
// Native OpenAPI support (.NET 9+)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Set API metadata
        document.Info = new OpenApiInfo
        {
            Title = "Factory API",
            Version = "v1",
            Description = "API for IoT device and factory management",
            Contact = new OpenApiContact
            {
                Name = "Factory API Team",
                Email = "support@factory.com"
            }
        };

        // Add JWT Bearer authentication scheme
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.",
            In = ParameterLocation.Header,
            Name = "Authorization"
        };

        // Apply security requirement globally
        document.Security = [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            }
        ];

        return Task.CompletedTask;
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

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

// Register repositories (Phase 3) - All SOLID compliant
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IMachineStatusRepository, MachineStatusRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register application services
//builder.Services.AddScoped<IAuthService, PasswordService>();

// Build the app
var app = builder.Build();
app.MapOpenApi();

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

// Add authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapHealthChecks("/health");

// Map MVC controllers
app.MapControllers();

// Map gRPC services
app.MapGrpcService<MachineGatewayService>();

app.MapGet("/", () => "Device Gateway running. REST: /api, gRPC: DeviceGateway");

app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Factory API")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.Run();

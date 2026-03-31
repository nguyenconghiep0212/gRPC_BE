using IotGrpcLearning.GrpcServices;
using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
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
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
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

builder.AddSingleton();

// Validate and register Sqlite connection factory
builder.AddValidatedSqlite();

// Health checks
builder.Services.AddHealthChecks();

// Configure PasswordOptions from configuration
builder.Services.Configure<PasswordOptions>(builder.Configuration.GetSection("Password"));

  
// Build the app
var app = builder.Build(); 
app.UseSwagger();
app.UseSwaggerUI();

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

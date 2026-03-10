using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IotGrpcLearning.Infrastructure
{
    /// <summary>
    /// Minimal global exception middleware that logs and returns a consistent JSON error response.
    /// Add to pipeline early with: app.UseMiddleware&lt;ExceptionMiddleware&gt;();
    /// </summary>
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _log;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException oce) when (context.RequestAborted.IsCancellationRequested)
            {
                _log.LogInformation(oce, "Request was canceled by the client.");
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            }
            catch (Exception ex)
            {
                var id = Guid.NewGuid().ToString("N");
                _log.LogError(ex, "Unhandled exception (id={CorrelationId})", id);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = new
                {
                    Error = "InternalServerError",
                    Message = "An unexpected error occurred.",
                    CorrelationId = id
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
}
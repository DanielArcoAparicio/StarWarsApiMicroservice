using System.Diagnostics;
using StarWars.Application.Interfaces;

namespace StarWars.Api.Middleware;

/// <summary>
/// Middleware para registrar todas las peticiones en el historial
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRequestHistoryService historyService)
    {
        // Ignorar health checks y swagger
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/health") || path.Contains("/swagger"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Registrar la petición
            var endpoint = $"{context.Request.Path}{context.Request.QueryString}";
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            await historyService.LogRequestAsync(
                endpoint,
                method,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                context.Request.QueryString.ToString(),
                null,
                ipAddress);

            _logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                method,
                endpoint,
                stopwatch.ElapsedMilliseconds,
                statusCode);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var endpoint = $"{context.Request.Path}{context.Request.QueryString}";
            var method = context.Request.Method;

            await historyService.LogRequestAsync(
                endpoint,
                method,
                500,
                stopwatch.ElapsedMilliseconds,
                context.Request.QueryString.ToString(),
                ex.Message,
                context.Connection.RemoteIpAddress?.ToString());

            throw;
        }
    }
}


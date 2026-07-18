using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Storefront.Api.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns an RFC 7807 ProblemDetails response.
/// Stack traces are included only in the Development environment.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Correlation ID for log tracing (reuse existing or generate new)
        var traceId = context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception on {Method} {Path}. TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            traceId);

        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException        => (HttpStatusCode.NotFound,     "Resource not found"),
            ArgumentException           => (HttpStatusCode.BadRequest,   "Invalid argument"),
            OperationCanceledException  => (HttpStatusCode.BadRequest,   "Request cancelled"),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        var problem = new ProblemDetails
        {
            Type     = $"https://httpstatuses.com/{(int)statusCode}",
            Title    = title,
            Status   = (int)statusCode,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId,
                ["requestId"] = context.Request.Headers["X-Request-Id"].FirstOrDefault() ?? traceId
            }
        };

        // Only expose detail and stack trace outside Production to avoid information leakage
        if (!_env.IsProduction())
        {
            problem.Detail = exception.Message;
            problem.Extensions["stackTrace"] = exception.StackTrace;
            problem.Extensions["exceptionType"] = exception.GetType().FullName;

            if (exception.InnerException is not null)
                problem.Extensions["innerException"] = exception.InnerException.Message;
        }
        else
        {
            // Generic message in production — specifics go to structured logs only
            problem.Detail = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again or contact support."
                : title;
        }

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        // Prevent response caching for error responses
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Pragma       = "no-cache";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, JsonOptions));
    }
}

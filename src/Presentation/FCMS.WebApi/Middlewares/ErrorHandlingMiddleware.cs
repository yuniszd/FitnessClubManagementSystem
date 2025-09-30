using FCMS.Application.Extensions.Exceptions;
using System.Net;
using System.Text.Json;

namespace FCMS.WebApi.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Loglama: centralized, production-ready
            _logger.LogError(ex, "❌ Global exception caught");

            // Exception-u handle et
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        int statusCode = ex switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            ValidationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        string errorCode = ex switch
        {
            BaseException be => be.ErrorCode,
            _ => ErrorCodes.InternalError
        };

        string userMessage = ex switch
        {
            BaseException be => be.UserMessage,
            _ => "An unexpected error occurred"
        };

        string? details = ex is ValidationException vex
            ? string.Join("; ", vex.Errors)
            : ex is BaseException bex
                ? bex.Details
                : null;



        var response = new
        {
            success = false,
            statusCode,
            errorCode,
            message = ex.Message,
            userMessage,
            details,
            path = context.Request.Path,
            timestamp = DateTime.UtcNow,
            stackTrace = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? ex.StackTrace : null
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));

    }
}
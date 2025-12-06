using System.Text.Json;
using API.Database;
using API.Models.Constants;

namespace API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items[ContextKeys.CorrelationId]?.ToString() ?? Guid.NewGuid().ToString();

            var (statusCode, message) = ex switch
            {
                ConcurrencyException => (StatusCodes.Status409Conflict, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, env.IsDevelopment() ? ex.Message : "Internal Server Error")
            };

            logger.LogError(ex, "Exception [{CorrelationId}]: {Message}", correlationId, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiException(statusCode, message, $"CorrelationId: {correlationId}");
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}

public record ApiException(int StatusCode, string Message, string? Detail = null);

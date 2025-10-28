using System.Text.Json;

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
            var correlationId = Guid.NewGuid().ToString();
            logger.LogError(ex, "Unhandled Exception [{CorrelationId}]: {Message}", correlationId, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            var response = env.IsDevelopment() 
                ? new ApiException(context.Response.StatusCode, ex.Message, $"{ex.GetType().Name} (CorrelationId: {correlationId})")
                : new ApiException(context.Response.StatusCode, "Internal Service Error", $"CorrelationId: {correlationId}");
            
            var options = new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}

public record ApiException(int StatusCode, string Message, string? Detail = null);

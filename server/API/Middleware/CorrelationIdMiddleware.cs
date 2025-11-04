using API.Models.Constants;

namespace API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderNames.CorrelationId].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        
        context.Items[ContextKeys.CorrelationId] = correlationId;
        
        context.Response.Headers[HeaderNames.CorrelationId] = correlationId;
        
        await _next(context);
    }
}
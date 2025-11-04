using API.Models.Constants;
using Serilog.Context;

namespace API.Middleware;

public class SerilogEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public SerilogEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items[ContextKeys.CorrelationId]?.ToString();

        if (!string.IsNullOrEmpty(correlationId))
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
namespace API.Middleware;

public class RateLimitHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Register a callback to run after the response starts
        context.Response.OnStarting(() =>
        {
            // Add Retry-After header for rate limit rejections
            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                if (!context.Response.Headers.ContainsKey("Retry-After"))
                    context.Response.Headers.RetryAfter = "60";
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
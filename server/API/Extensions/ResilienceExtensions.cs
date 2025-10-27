using Polly;

namespace API.Extensions;

public static class ResilienceExtensions
{
    public static IServiceCollection AddPollyPolicies(this IServiceCollection services)
    {
        services.AddHttpClient("PollyClient").AddPolicyHandler((serviceProvider, request) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            
            // Retry Policy: 3 times with exponential backoff (2^retryAttempt seconds)
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .OrResult(response => !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (result, timeSpan, retryCount, context) =>
                    {
                        var reason = result.Exception?.Message ?? $"HTTP {(int?)result.Result?.StatusCode} {result.Result?.ReasonPhrase}";
                        logger.LogWarning("Retry {RetryCount} after {TimeSpanTotalSeconds} seconds due to: {Reason}",
                            retryCount, timeSpan.TotalSeconds, reason);
                    }
                );

            // Circuit Breaker: Stop trying after 5 failures, wait 30s before retry
            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => !response.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (result, duration) =>
                    {
                        var reason = result.Exception?.Message ?? $"HTTP {(int?)result.Result?.StatusCode} {result.Result?.ReasonPhrase}";
                        logger.LogError("Circuit breaker opened for {DurationTotalSeconds} seconds due to: {Reason}",
                            duration.TotalSeconds, reason);
                    },
                    onReset: () => logger.LogInformation("Circuit breaker reset"),
                    onHalfOpen: () => logger.LogInformation("Circuit breaker half-open, testing...")
                );
            
            // Timeout Policy: Cancel operations taking longer than 10 seconds
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
            
            // Combine Policies: Timeout -> Retry -> Circuit Breaker (wrap is reversed)
            // Circuit Breaker checks if circuit is open, Retry retries if closed, Timeout cuts off each attempt after 10s
            return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);
        });
        
        return services;
    }
}
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using API.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                config.GetConnectionString("DefaultConnection")!,
                healthQuery: "SELECT 1;",
                name: "sql-server",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "ready"],
                timeout: TimeSpan.FromSeconds(3))
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: ["self"]);
        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<RateLimitingOptions>()
            .BindConfiguration(RateLimitingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var rateLimitOptions = config.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() 
                               ?? throw new InvalidOperationException("RateLimiting configuration is missing.");
        
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // 5 requests per minute per IP to prevent brute force attacks
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(rateLimitOptions.Auth.WindowMinutes),
                        PermitLimit = rateLimitOptions.Auth.PermitLimit,
                        QueueLimit = 0
                    }));

            // Start with 100 tokens, refill 50 per minute
            // Allow bursts but then throttle to 50 req/minute sustained
            options.AddPolicy("authenticated", context =>
            {
                var userId = context.User.FindFirst("userId")?.Value;
                var partitionKey = userId ?? GetClientIdentifier(context);

                return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = rateLimitOptions.Authenticated.TokenCapacity,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    TokensPerPeriod = rateLimitOptions.Authenticated.TokensPerMinute,
                    QueueLimit = 0,
                });
            });

            // 60 requests per minute per IP, sliding window with 6 segments (10-second segments)
            // Requests distributed more evenly so fairer than fixed window
            options.AddPolicy("anonymous", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(rateLimitOptions.Anonymous.WindowMinutes),
                        PermitLimit = rateLimitOptions.Anonymous.PermitLimit,
                        SegmentsPerWindow = rateLimitOptions.Anonymous.SegmentsPerWindow,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static IServiceCollection AddResponseCachingConfiguration(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<CachingOptions>()
            .BindConfiguration(CachingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var cachingOptions = config.GetSection(CachingOptions.SectionName).Get<CachingOptions>() 
                             ?? throw new InvalidOperationException("Caching configuration is missing");
        
        // StaticData includes price types, product types, and categories (rarely change)
        // NOTE: Tags are also cached but the duration is set at the endpoint
        services.AddOutputCache(options =>
        {
            options.AddPolicy("StaticData", builder => builder.Expire(TimeSpan.FromDays(cachingOptions.StaticDataExpirationDays)));
        });
        return services;
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Support test isolation with custom header
        var testId = context.Request.Headers["Test-Partition-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(testId))
            return $"test-{testId}";
        
        // Check X-Forwarded-For header (Azure still uses it even though X- is deprecated)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();
        
        // Fallback to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
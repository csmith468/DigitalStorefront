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

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(rateLimitOptions.WindowMinutes),
                        PermitLimit = rateLimitOptions.PermitLimit,
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
}
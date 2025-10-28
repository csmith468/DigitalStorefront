using System.Threading.RateLimiting;
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

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 100,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static IServiceCollection AddResponseCachingConfiguration(this IServiceCollection services)
    {
        // StaticData includes price types, product types, and categories (rarely change)
        services.AddOutputCache(options =>
        {
            options.AddPolicy("StaticData", builder => builder.Expire(TimeSpan.FromDays(1)));
        });
        return services;
    }
}
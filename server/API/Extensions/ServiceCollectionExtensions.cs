using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DevCors", corsBuilder =>
            {
                corsBuilder.WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
            // options.AddPolicy("ProdCors", (corsBuilder) =>
            //     {
            //         corsBuilder.WithOrigins("eventual link")
            //             .AllowAnyMethod()
            //             .AllowAnyHeader()
            //             .AllowCredentials();
            //     });
        });

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var configTokenKey = config.GetSection("AppSettings:TokenKey").Value;
        if (configTokenKey is null) throw new Exception("Cannot create token.");
        var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configTokenKey));

        var tokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = tokenKey,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        // NOTE: I hard-coded role names here to match the database exactly, but for production with roles 
        // that may change a lot, I'd likely use an enum as a single source of truth and seed the database from 
        // the enum on startup. This would let me autogenerate these policies with Enum.GetValues<UserRole>()
        // and get rid of hard-coding. I decided the tradeoff wasn't necessary for the 3 roles in this project
        // ADMIN Meaning: Can edit demo products
        
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("CanManageProducts", policy => policy.RequireRole("Admin", "ProductWriter"));
            options.AddPolicy("CanManageImages", policy => policy.RequireRole("Admin", "ImageManager"));
        });
        
        return services;
    }

    public static IServiceCollection AddSwaggerAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });
        
        return services;
    }
    
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

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
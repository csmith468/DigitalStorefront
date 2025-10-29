using System.IdentityModel.Tokens.Jwt;
using System.Text;
using API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class SecurityExtensions
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
    
    public static IServiceCollection AddSecurityOptions(this IServiceCollection services)
    {
        services.AddOptions<SecurityOptions>()
            .BindConfiguration(SecurityOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
    
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var tokenKey = config.GetSection("AppSettings:TokenKey").Value
                       ?? throw new InvalidOperationException("AppSettings:TokenKey is not configured");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var tokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = securityKey,
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
}
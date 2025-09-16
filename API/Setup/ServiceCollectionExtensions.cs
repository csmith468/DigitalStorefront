using System.Reflection;
using System.Text;
using API.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Setup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutoRegistration(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var implementationType in types)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

            if (interfaceType != null)
                services.AddScoped(interfaceType, implementationType);
        }

        return services;
    }

    public static IServiceCollection AddSharedContainer(this IServiceCollection services)
    {
        services.AddScoped<IDataContextDapper, DataContextDapper>();
        services.AddScoped<ISharedContainer, SharedContainer>();
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
            ValidateIssuerSigningKey = false,
            ValidateAudience = false,
        };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
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
}
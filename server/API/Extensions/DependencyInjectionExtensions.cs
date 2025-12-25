using System.Reflection;
using API.Database;
using API.Infrastructure.Contexts;
using API.Infrastructure.Startup;
using API.Infrastructure.Viewers;
using API.Services;
using API.Services.Images;
using API.Utils;

namespace API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAutoRegistration(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
            .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Context"))
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

    public static IServiceCollection AddManualRegistrations(this IServiceCollection services)
    {
        services.AddScoped<DataContextDapper>();
        services.AddScoped<IQueryExecutor>(sp => sp.GetRequiredService<DataContextDapper>());
        services.AddScoped<ICommandExecutor>(sp => sp.GetRequiredService<DataContextDapper>());
        services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<DataContextDapper>());
        
        services.AddScoped<IAuditContext, HttpAuditContext>();
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
        services.AddScoped<ICacheWarmer, CacheWarmer>();
        services.AddScoped<IRoleSeeder, RoleSeeder>();
        services.AddScoped<TokenGenerator>();
        services.AddScoped<PasswordHasher>();

        // replaces scoped auto-registration
        services.AddSingleton<IViewerTrackingService, ViewerTrackingService>();
        return services;
    }
    
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        // Scans all loaded assemblies for AutoMapper Profile classes
        services.AddAutoMapper(typeof(Program).Assembly);

        return services;
    }
}
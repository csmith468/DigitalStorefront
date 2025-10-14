using System.Reflection;
using API.Database;
using API.Services.Images;
using API.Setup;

namespace API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAutoRegistration(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
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

    public static IServiceCollection AddManualRegistrations(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddScoped<IDataContextDapper, DataContextDapper>();
        services.AddScoped<ISharedContainer, SharedContainer>();
        
        return services;
    }
}
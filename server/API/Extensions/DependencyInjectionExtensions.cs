using System.Reflection;
using API.Database;
using API.Services;
using API.Services.Contexts;
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
        services.AddScoped<TokenGenerator>();
        services.AddScoped<PasswordHasher>();
        return services;
    }

    public static IServiceCollection AddImageStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var useAzureStorage = configuration.GetValue("UseAzureStorage", false);

        if (useAzureStorage)
        {
            services.AddOptions<Configuration.AzureBlobStorageOptions>()
                .BindConfiguration(Configuration.AzureBlobStorageOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString)
                                     && !string.IsNullOrWhiteSpace(options.ContainerName), 
                    """
                    AzureBlobStorage configuration is incomplete. ConnectionString and ContainerName are required 
                    when UseAzureStorage is enabled. Check your appsettings.json or Azure App Service Configuration.
                    """)
                .ValidateOnStart();
            
            services.AddScoped<IImageStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IImageStorageService, LocalImageStorageService>();
            services.AddScoped<IStoragePathProvider, WebStoragePathProvider>();
        }

        return services;
    }
    
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        // Scans all loaded assemblies for AutoMapper Profile classes
        services.AddAutoMapper(typeof(Program).Assembly);

        return services;
    }
}
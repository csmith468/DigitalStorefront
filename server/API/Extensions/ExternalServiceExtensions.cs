using API.Configuration;
using API.Services.Images;
using SendGrid;
using Stripe;

namespace API.Extensions;

public static class ExternalServiceExtensions
{
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

    public static IServiceCollection AddStripe(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<StripeOptions>()
            .BindConfiguration(StripeOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        return services;
    }

    public static IServiceCollection AddSendGrid(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<SendGridOptions>()
            .BindConfiguration(SendGridOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ISendGridClient>(sp =>
        {
            var options = config.GetSection(SendGridOptions.SectionName).Get<SendGridOptions>();
            return new SendGridClient(options!.ApiKey);
        });

        return services;
    }
}
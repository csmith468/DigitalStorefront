using API.Database;
using API.Extensions;
using API.Filters;
using API.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApplicationInsightsTelemetry();
    builder.ConfigureSerilog();

    // Validate Custom Attributes
    DbAttributeValidator.ValidateAllEntities(typeof(Program).Assembly);

    // Framework Services
    builder.Services.AddScoped<ActionTimingFilter>();
    builder.Services.AddControllers(options => options.Filters.Add<ActionTimingFilter>());
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpContextAccessor();

    // Security
    builder.Services.AddCorsConfiguration(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies(); 
    builder.Services.AddSecurityOptions();

    // Validation & Documentation
    builder.Services.AddValidation();
    builder.Services.AddSwaggerAuthorization();

    // Infrastructure
    builder.Services.AddHealthChecksConfiguration(builder.Configuration);
    builder.Services.AddRateLimiting(builder.Configuration);
    builder.Services.AddResponseCachingConfiguration(builder.Configuration);
    builder.Services.AddDirectoryBrowser();

    // Dependency Injection
    builder.Services.AddAutoRegistration(typeof(Program).Assembly);
    builder.Services.AddManualRegistrations();
    builder.Services.AddImageStorage(builder.Configuration);
    builder.Services.AddMappings();

    // Resilience
    builder.Services.AddPollyPolicies();

    var app = builder.Build();

    await app.Services.EnsureRolesSeededAsync();

    // Middleware
    app.UseMiddleware<CorrelationIdMiddleware>();       // generate/preserve
    app.UseMiddleware<SerilogEnrichmentMiddleware>();   // add to log context
    app.UseSerilogRequestLoggingWithEnrichment();       // log HTTP requests
    app.UseMiddleware<ExceptionMiddleware>();           // catch exceptions (logs include correlation ID)

    // Configure HTTP Request Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevCors");
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseCors("ProdCors");
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();

    app.UseMiddleware<RateLimitHeadersMiddleware>();
    app.UseRateLimiter();
    app.UseOutputCache();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthCheckEndpoints();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program { }

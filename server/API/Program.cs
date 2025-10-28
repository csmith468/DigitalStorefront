using API.Database;
using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Validate Custom Attributes
DbAttributeValidator.ValidateAllEntities(typeof(Program).Assembly);

// Framework Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Security
builder.Services.AddCorsConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddSecurityOptions();

// Validation & Documentation
builder.Services.AddValidation();
builder.Services.AddSwaggerAuth();

// Infrastructure
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddRateLimiting();
builder.Services.AddResponseCachingConfiguration();
builder.Services.AddDirectoryBrowser();

// Dependency Injection
builder.Services.AddAutoRegistration(typeof(Program).Assembly);
builder.Services.AddManualRegistrations();
builder.Services.AddMappings();

// Resilience
builder.Services.AddPollyPolicies();

var app = builder.Build();

// Exception Handling
app.UseMiddleware<ExceptionMiddleware>();

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

app.UseRateLimiter();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthCheckEndpoints();
app.MapControllers();

app.Run();

public partial class Program { }

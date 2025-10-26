using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Framework Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Application Services
builder.Services.AddCorsConfiguration();
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddApiVersioningConfig();
builder.Services.AddValidation();
builder.Services.AddSwaggerAuth();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Dependency Injection
builder.Services.AddAutoRegistration(typeof(Program).Assembly);
builder.Services.AddManualRegistrations();
builder.Services.AddMappings();

// Resilience
builder.Services.AddPollyPolicies();

var app = builder.Build();

// Configure Static Files
builder.Services.AddDirectoryBrowser();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthCheckEndpoints();
app.MapControllers();

app.Run();

public partial class Program { }

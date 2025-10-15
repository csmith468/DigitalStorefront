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
builder.Services.AddApiVersioningConfig();
builder.Services.AddSwaggerAuth();

// Dependency Injection
builder.Services.AddAutoRegistration(typeof(Program).Assembly);
builder.Services.AddManualRegistrations();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

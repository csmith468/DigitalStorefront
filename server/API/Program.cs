using API.Database;
using API.Middleware;
using API.Setup;

var builder = WebApplication.CreateBuilder(args);

 // Framework Services
  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();

  // Application Services
  builder.Services.AddAutoRegistration(typeof(Program).Assembly);
  builder.Services.AddSharedContainer();

  // Security & CORS
  builder.Services.AddAuthentication(builder.Configuration);
  builder.Services.AddCorsConfiguration();

  // Swagger
  builder.Services.AddSwaggerAuth();

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


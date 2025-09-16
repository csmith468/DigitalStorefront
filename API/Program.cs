using API.Database;
using API.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerAuth();

builder.Services.AddControllers();

builder.Services.AddAutoRegistration(typeof(Program).Assembly);
builder.Services.AddSharedContainer();
builder.Services.AddAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors((options) =>
    {
        options.AddPolicy("DevCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        // options.AddPolicy("ProdCors", (corsBuilder) =>
        //     {
        //         corsBuilder.WithOrigins("eventual link")
        //             .AllowAnyMethod()
        //             .AllowAnyHeader()
        //             .AllowCredentials();
        //     });
    });
 
var app = builder.Build();
 
// Configure the HTTP request pipeline.
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


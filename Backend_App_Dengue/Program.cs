using System.Net;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend_App_Dengue",
        Version = "v1",
        Description = "API para la gesti�n de datos de dengue."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend_App_Dengue v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Habilitar CORS
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

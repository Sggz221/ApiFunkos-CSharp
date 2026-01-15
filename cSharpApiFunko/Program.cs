using cSharpApiFunko.DataBase;
using cSharpApiFunko.Repositories.Categorias;
using cSharpApiFunko.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddDbContext<Context>(options => 
    options.UseInMemoryDatabase("FunkoDb"));

builder.Services.AddScoped<IFunkoRepository, FunkoRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IFunkoService, FunkoService>();

builder.Services.AddMemoryCache();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensaje = string.Join(", ", context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        return new BadRequestObjectResult(new { message = mensaje });
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Inicializando Base de Datos...");
    // Esta línea dispara el OnModelCreating y el SeedData ANTES de que la API acepte peticiones
    context.Database.EnsureCreated(); 
    logger.LogInformation("Base de Datos lista.");
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "Error inesperado: " + ex.Message });
    }
});

app.UseHttpsRedirection();

app.UseAuthorization(); 

app.MapControllers(); 

app.Run();
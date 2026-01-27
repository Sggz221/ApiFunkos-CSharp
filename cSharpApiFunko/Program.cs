using System.Threading.Channels;
using cSharpApiFunko.DataBase;
using cSharpApiFunko.Email;
using cSharpApiFunko.Notifications;
using cSharpApiFunko.Repositories.Categorias;
using cSharpApiFunko.Services.Categorias;
using cSharpApiFunko.Services.Funkos;
using cSharpApiFunko.Storage;
using FunkoApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cSharpApiFunko.GraphQl;
using cSharpApiFunko.GraphQl.Mutation;
using cSharpApiFunko.GraphQl.Types;
using cSharpApiFunko.Errors;
using cSharpApiFunko.GraphQl.Query;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Controladores (REST)
builder.Services.AddControllers();

// Base de Datos
builder.Services.AddDbContext<Context>(options => 
    options.UseInMemoryDatabase("FunkoDb"));

// Inyección de Dependencias (Servicios y Repositorios)
builder.Services.AddScoped<IFunkoRepository, FunkoRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IFunkoService, FunkoService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStorageService, FileSystemStorageService>();

//Email Cambiado a MailKit para enviar emails reales a Mailtrap
// Canal para comunicación entre servicios (cola de emails)
builder.Services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());
// Servicio de background que procesa la cola de emails
builder.Services.AddHostedService<EmailBackgroundService>();
// Servicio de email
builder.Services.TryAddScoped<IEmailService, MailKitEmailService>();

// Cache y Configuración de Errores de Modelo
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

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// CONFIGURACIÓN DE GRAPHQL SERVER
builder.Services
    .AddGraphQLServer()
    .AddQueryType<FunkoQuery>()
    .AddMutationType<FunkoMutation>()
    .AddSubscriptionType<FunkoSubscription>()
    .AddType<FunkoType>()
    // .AddType<CategoryType>()
    .AddType<FunkoError>()       
    .AddType<NotFoundError>()
    .AddType<ValidationError>()
    .AddType<ConflictError>()
    .AddType<StorageError>()
    .AddInMemorySubscriptions()
    .AddAuthorization()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

var app = builder.Build();

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

// Inicializador de BD (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Inicializando Base de Datos...");
    context.Database.EnsureCreated(); 
    logger.LogInformation("Base de Datos lista.");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configuración de CORS
app.UseCors("AllowSignalR");

app.UseAuthorization(); 

// REST Controllers
app.MapControllers();

// SignalR Hubs
app.MapHub<FunkoHub>("/hubs/funkos");

// GRAPHQL
app.UseWebSockets(); 
app.MapGraphQL();

// Mensaje en consola
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("GraphQL Playground disponible en: http://localhost:5180/graphql");
}

app.Run();
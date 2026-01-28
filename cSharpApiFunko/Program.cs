using cSharpApiFunko.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Controladores (REST)
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// Configuraciones modulares
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddEmailServices();
builder.Services.AddSecurityConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddGraphQLConfiguration();

var app = builder.Build();

// Middleware de manejo global de errores
app.UseGlobalErrorHandling();

// Inicialización de Base de Datos
app.InitializeDatabase();

// Pipeline de middleware de la aplicación
app.UseApplicationMiddleware();

// Mensaje en consola
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("GraphQL Playground disponible en: http://localhost:5180/graphql");
}

app.Run();

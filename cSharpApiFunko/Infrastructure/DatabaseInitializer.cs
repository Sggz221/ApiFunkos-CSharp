using cSharpApiFunko.DataBase;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Inicializador de base de datos
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Inicializa la base de datos (Seed Data)
    /// </summary>
    public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Inicializando Base de Datos...");
            context.Database.EnsureCreated();
            logger.LogInformation("Base de Datos lista.");
        }
        
        return app;
    }
}

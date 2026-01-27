using cSharpApiFunko.DataBase;
using Microsoft.EntityFrameworkCore;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración de la base de datos
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configura el contexto de base de datos con PostgreSQL
    /// </summary>
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Prioridad: DATABASE_URL (Docker) > DefaultConnection (local) > fallback
        var connectionString = configuration["DATABASE_URL"] 
                              ?? configuration.GetConnectionString("DefaultConnection") 
                              ?? "Host=localhost;Port=5432;Database=funko_db;Username=funko_user;Password=funko_password";
        
        services.AddDbContext<Context>(options =>
        {
            options.UseNpgsql(connectionString);
            options.EnableSensitiveDataLogging(); // Para desarrollo
            options.EnableDetailedErrors(); // Para desarrollo
        });
        
        return services;
    }
}

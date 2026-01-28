using cSharpApiFunko.Repositories.Categorias;
using cSharpApiFunko.Repositories.Usuarios;
using cSharpApiFunko.Services.Auth;
using cSharpApiFunko.Services.Categorias;
using cSharpApiFunko.Services.Funkos;
using cSharpApiFunko.Storage;
using FunkoApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración de inyección de dependencias para servicios y repositorios
/// </summary>
public static class DependencyInjectionConfiguration
{
    /// <summary>
    /// Registra todos los servicios y repositorios en el contenedor de DI
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositorios
        services.AddScoped<IFunkoRepository, FunkoRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        // Servicios
        services.AddScoped<IFunkoService, FunkoService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IJwtTokenExtractor, JwtTokenExtractor>();
        services.AddScoped<IStorageService, FileSystemStorageService>();
        
        // Cache - Redis Distributed
        services.AddStackExchangeRedisCache(options =>
        {
            // Prioridad: REDIS_CONNECTION (Docker) > ConnectionStrings:Redis (local) > fallback
            var redisConnection = configuration["REDIS_CONNECTION"] 
                                ?? configuration.GetConnectionString("Redis") 
                                ?? "localhost:6379";
            
            options.Configuration = redisConnection;
            options.InstanceName = "FunkoCache:";
        });
        
        // Configuración de errores de validación de modelo
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var mensaje = string.Join(", ", context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return new BadRequestObjectResult(new { message = mensaje });
            };
        });
        
        return services;
    }
}

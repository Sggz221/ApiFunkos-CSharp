namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración de políticas CORS
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Configura las políticas CORS para permitir SignalR
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSignalR", policy =>
            {
                policy.SetIsOriginAllowed(origin => true) 
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        return services;
    }
}

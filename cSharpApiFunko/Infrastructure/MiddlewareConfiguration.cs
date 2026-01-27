using cSharpApiFunko.Notifications;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración de middleware de la aplicación
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// Configura el middleware global de manejo de errores
    /// </summary>
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
    {
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
        
        return app;
    }
    
    /// <summary>
    /// Configura el pipeline de middleware de la aplicación
    /// </summary>
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        // CORS
        app.UseCors("AllowSignalR");
        
        app.UseAuthorization();
        
        // REST Controllers
        app.MapControllers();
        
        // SignalR Hubs
        app.MapHub<FunkoHub>("/hubs/funkos");
        
        // GraphQL
        app.UseWebSockets();
        app.MapGraphQL();
        
        return app;
    }
}
